using Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;
using Pos.SalesService.Application.Wrappers;
using Microsoft.EntityFrameworkCore;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Infrastructure.Persistence.Contexts;
using Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

namespace Pos.ReportingService.Infrastructure.Persistence.Repositories;

public class SalesReportRepositoryAsync : ISalesReportRepositoryAsync
{
    private readonly ApplicationDbContext _context;
    public SalesReportRepositoryAsync(ApplicationDbContext context) { _context = context; }

    public async Task<IReadOnlyList<DailySalesReportDto>> GetDailySalesReportAsync(Guid tenantId, SalesReportFilter filter, CancellationToken cancellationToken)
    {
        // 1) Sales: one row per day
        var salesByDate= await _context.Sales
            .AsNoTracking()
            .ApplySalesFilter(tenantId,filter)
            .GroupBy(s=>s.CompletedAt!.Value.Date)
            .Select(g=>new
            {
                Date=g.Key,
                Count=g.Count(),
                Total=g.Sum(s=>s.Total),
                Discount = g.Sum(s => s.DiscountTotal),
                Tax = g.Sum(s => s.TaxTotal)
            })
            .ToDictionaryAsync(x => x.Date, cancellationToken);

        // 2) Returns: one row per day, aggregated in SQL
        var returnsByDate = await _context.Returns
            .AsNoTracking()
            .ApplyReturnsFilter(_context.Sales, tenantId, filter)
            .GroupBy(r => r.CompletedAt.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(r => r.RefundAmount) })
            .ToDictionaryAsync(x => x.Date, x => x.Amount, cancellationToken);


        return salesByDate.Keys
        .Union(returnsByDate.Keys)
        .OrderBy(d => d)
        .Select(d =>
        {
            salesByDate.TryGetValue(d, out var s);
            returnsByDate.TryGetValue(d, out var returned);

            var salesTotal = s?.Total ?? 0m;

            return new DailySalesReportDto
            {
                Date = DateOnly.FromDateTime(d),
                CurrencyCode = filter.CurrencyCode,
                CompletedSaleCount = s?.Count ?? 0,
                SalesTotal = salesTotal,
                DiscountTotal = s?.Discount ?? 0m,
                SalesTaxTotal = s?.Tax ?? 0m,
                AcceptedReturnAmount = returned,
                SalesTotalAfterReturns = salesTotal - returned
            };
        })
        .ToList();

    }

    public async Task<SalesDashboardDto> GetDashboardAsync(Guid tenantId, SalesReportFilter filter, CancellationToken cancellationToken)
    {
        var result = await _context.Sales.ApplySalesFilter(tenantId, filter)
            .GroupBy(x => x.CurrencyCode)
            .Select(group => new SalesDashboardDto
            {
                CurrencyCode = group.Key,
                CompletedSaleCount = group.Count(),
                SalesTotal = group.Sum(x => x.Total),
                DiscountTotal = group.Sum(x => x.DiscountTotal),
                SalesTaxTotal = group.Sum(x => x.TaxTotal)
            }).SingleOrDefaultAsync(cancellationToken)
            ?? new SalesDashboardDto { CurrencyCode = filter.CurrencyCode };


        result.AcceptedReturnAmount = await _context.Returns
            .ApplyReturnsFilter(_context.Sales, tenantId, filter)
            .SumAsync(x => (decimal?)x.RefundAmount, cancellationToken) ?? 0m;

        result.SalesTotalAfterReturns = result.SalesTotal - result.AcceptedReturnAmount;

        return result;
    }

    public async Task<PagedResponse<IEnumerable<ProductSalesReportDto>>> GetProductSalesReportAsync(
        Guid tenantId, GetProductSalesReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        var sales = BuildProductSalesQuery(tenantId, parameter.Filter);

        var returns = BuildProductReturnsQuery(tenantId, parameter.Filter);

        var report = BuildProductReportQuery(sales.Concat(returns), parameter.Filter.CurrencyCode);

        var totalCount = await report.CountAsync(cancellationToken);

        var offset = ((long)parameter.PageNumber - 1) * parameter.PageSize;

        if (offset >= totalCount)
            return new PagedResponse<IEnumerable<ProductSalesReportDto>>(
                Array.Empty<ProductSalesReportDto>(), parameter.PageNumber, parameter.PageSize, totalCount);

        var rows = await report.ApplyOrder(parameter)
            .Skip((int)offset).Take(parameter.PageSize).ToListAsync(cancellationToken);

        await AddProductNamesAsync(tenantId, rows, cancellationToken);

        return new PagedResponse<IEnumerable<ProductSalesReportDto>>(
            rows, parameter.PageNumber, parameter.PageSize, totalCount);
    }

    private IQueryable<ProductSalesContribution> BuildProductSalesQuery(Guid tenantId, ProductSalesReportFilter filter)
    {
        var sales = _context.Sales.ApplySalesFilter(tenantId, filter);

        var items = _context.SaleItems.ApplyProductFilter(tenantId, filter);

        return from sale in sales
               join item in items on new { sale.TenantId, SaleId = sale.Id }
                   equals new { item.TenantId, item.SaleId }
               group item by new { item.ProductId, item.ProductVariantId } into product
               select new ProductSalesContribution
               {
                   ProductId = product.Key.ProductId,
                   ProductVariantId = product.Key.ProductVariantId,
                   QuantitySold = product.Sum(x => x.Quantity),
                   QuantityReturned = 0m,
                   SalesTotal = product.Sum(x => x.LineTotal),
                   DiscountTotal = product.Sum(x => x.DiscountAmount),
                   SalesTaxTotal = product.Sum(x => x.TaxAmount),
                   ReturnAmount = 0m
               };
    }

    private IQueryable<ProductSalesContribution> BuildProductReturnsQuery(Guid tenantId, ProductSalesReportFilter filter)
    {
        var returns = _context.Returns.ApplyReturnsFilter(_context.Sales, tenantId, filter);

        var items = _context.ReturnItems.ApplyProductFilter(tenantId, filter);

        return from returned in returns
               join item in items on new { returned.TenantId, ReturnId = returned.Id, returned.OriginalSaleId }
                   equals new { item.TenantId, item.ReturnId, item.OriginalSaleId }
               group item by new { item.ProductId, item.ProductVariantId } into product
               select new ProductSalesContribution
               {
                   ProductId = product.Key.ProductId,
                   ProductVariantId = product.Key.ProductVariantId,
                   QuantitySold = 0m,
                   QuantityReturned = product.Sum(x => x.Quantity),
                   SalesTotal = 0m,
                   DiscountTotal = 0m,
                   SalesTaxTotal = 0m,
                   ReturnAmount = product.Sum(x => x.RefundAmount)
               };
    }

    private static IQueryable<ProductSalesReportDto> BuildProductReportQuery(
        IQueryable<ProductSalesContribution> contributions, string currencyCode)
    {
        return contributions.GroupBy(x => new { x.ProductId, x.ProductVariantId })
            .Select(product => new ProductSalesReportDto
            {
                ProductId = product.Key.ProductId,
                ProductVariantId = product.Key.ProductVariantId,
                CurrencyCode = currencyCode,
                QuantitySold = product.Sum(x => x.QuantitySold),
                QuantityReturned = product.Sum(x => x.QuantityReturned),
                NetQuantitySold = product.Sum(x => x.QuantitySold) - product.Sum(x => x.QuantityReturned),
                SalesTotal = product.Sum(x => x.SalesTotal),
                DiscountTotal = product.Sum(x => x.DiscountTotal),
                SalesTaxTotal = product.Sum(x => x.SalesTaxTotal),
                AcceptedReturnAmount = product.Sum(x => x.ReturnAmount),
                SalesTotalAfterReturns = product.Sum(x => x.SalesTotal) - product.Sum(x => x.ReturnAmount)
            });
    }

    private async Task AddProductNamesAsync(Guid tenantId, List<ProductSalesReportDto> rows, CancellationToken cancellationToken)
    {
        var productIds = rows.Select(x => x.ProductId).Distinct().ToList();
        var variantIds = rows.Where(x => x.ProductVariantId.HasValue)
            .Select(x => x.ProductVariantId!.Value).Distinct().ToList();

        // One name lookup for the page, including inactive products; no per-row queries.
        var products = _context.Products.Where(x => x.TenantId == tenantId && productIds.Contains(x.Id))
            .Select(x => new { ProductId = x.Id, VariantId = (Guid?)null, Name = x.NameEn });

        var variants = _context.ProductVariants.Where(x => x.TenantId == tenantId &&
                productIds.Contains(x.ProductId) && variantIds.Contains(x.Id))
            .Select(x => new { x.ProductId, VariantId = (Guid?)x.Id, x.Name });

        var names = await products.Concat(variants)
            .ToDictionaryAsync(x => (x.ProductId, x.VariantId), x => x.Name, cancellationToken);

        foreach (var row in rows)
        {
            row.ProductName = names.GetValueOrDefault((row.ProductId, (Guid?)null))
                ?? $"Unavailable product ({row.ProductId})";
            row.VariantName = row.ProductVariantId.HasValue
                ? names.GetValueOrDefault((row.ProductId, row.ProductVariantId))
                    ?? $"Unavailable variant ({row.ProductVariantId})"
                : null;
        }
    }

    private sealed class ProductSalesContribution
    {
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal QuantityReturned { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal SalesTaxTotal { get; set; }
        public decimal ReturnAmount { get; set; }
    }
}
