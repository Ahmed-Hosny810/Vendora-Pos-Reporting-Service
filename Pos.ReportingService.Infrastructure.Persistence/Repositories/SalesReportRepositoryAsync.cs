using Pos.ReportingService.Infrastructure.Persistence.Services;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;
using Pos.SalesService.Application.Wrappers;
using Microsoft.EntityFrameworkCore;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Infrastructure.Persistence.Contexts;
using Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels.Constants;

namespace Pos.ReportingService.Infrastructure.Persistence.Repositories;

public class SalesReportRepositoryAsync : ISalesReportRepositoryAsync
{
    private readonly ApplicationDbContext _context;
    private readonly ReportingTimeZone _reportingTimeZone;
    public SalesReportRepositoryAsync(ApplicationDbContext context, ReportingTimeZone reportingTimeZone)
    {
        _context = context;
        _reportingTimeZone = reportingTimeZone;
    }

    public Task<IReadOnlyList<DailySalesReportDto>> GetDailySalesReportAsync(Guid tenantId, DailySalesReportFilter filter, CancellationToken cancellationToken)
        => BuildDailySalesAsync(tenantId, filter, int.MaxValue, cancellationToken);

    public Task<IReadOnlyList<DailySalesReportDto>> GetDailySalesForExportAsync(Guid tenantId, DailySalesReportFilter filter, int maxRows, CancellationToken cancellationToken)
        => BuildDailySalesAsync(tenantId, filter, checked(maxRows + 1), cancellationToken);

    private async Task<IReadOnlyList<DailySalesReportDto>> BuildDailySalesAsync(Guid tenantId, DailySalesReportFilter filter, int limit, CancellationToken cancellationToken)
    {
        var utcFilter = _reportingTimeZone.ToUtcFilter(filter);
        var sqlTimeZoneId = _reportingTimeZone.SqlTimeZoneId;
        // Keep filtering on UTC columns; convert only the grouping expression.
        // 1) Sales: one row per business day
        var salesByDate= await _context.Sales
            .AsNoTracking()
            .ApplySalesFilter(tenantId,utcFilter)
            .GroupBy(s => EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(s.CompletedAt!.Value, "UTC"), sqlTimeZoneId).Date)
            .Select(g=>new
            {
                Date=g.Key,
                Count=g.Count(),
                Total=g.Sum(s=>s.Total),
                Discount = g.Sum(s => s.DiscountTotal),
                Tax = g.Sum(s => s.TaxTotal)
            })
            .OrderBy(x => x.Date).Take(limit)
            .ToDictionaryAsync(x => x.Date, cancellationToken);

        // 2) Returns: one row per day, aggregated in SQL
        var returnsByDate = await _context.Returns
            .AsNoTracking()
            .ApplyReturnsFilter(_context.Sales, tenantId, utcFilter)
            .GroupBy(r => EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(r.CompletedAt, "UTC"), sqlTimeZoneId).Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(r => r.RefundAmount) })
            .OrderBy(x => x.Date).Take(limit)
            .ToDictionaryAsync(x => x.Date, x => x.Amount, cancellationToken);


        return salesByDate.Keys
        .Union(returnsByDate.Keys)
        .OrderBy(d => d).Take(limit)
        .Select(d =>
        {
            salesByDate.TryGetValue(d, out var s);
            returnsByDate.TryGetValue(d, out var returned);

            var salesTotal = s?.Total ?? 0m;

            return new DailySalesReportDto
            {
                Date = DateOnly.FromDateTime(d),
                CurrencyCode = filter.CurrencyCode ?? "EGP",
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
            ?? new SalesDashboardDto { CurrencyCode = filter.CurrencyCode ?? "EGP" };


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

        var report = BuildProductReportQuery(sales.Concat(returns), parameter.Filter.CurrencyCode ?? "EGP");

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

    public async Task<PagedResponse<IEnumerable<BranchSalesReportDto>>> GetBranchSalesReportAsync(
        Guid tenantId, GetBranchSalesReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        var report = BuildBranchSalesReportQuery(tenantId, parameter.Filter);

        var totalCount = await report.CountAsync(cancellationToken);

        var offset = ((long)parameter.PageNumber - 1) * parameter.PageSize;

        if (offset >= totalCount)
            return new PagedResponse<IEnumerable<BranchSalesReportDto>>(
                Array.Empty<BranchSalesReportDto>(), parameter.PageNumber, parameter.PageSize, totalCount);

        var rows = await report
            .ApplyOrder(parameter)
            .Skip((int)offset)
            .Take(parameter.PageSize)
            .ToListAsync(cancellationToken);

        var branchIds = rows.Select(x => x.BranchId).ToList();

        var names = await _context.Branches.Where(x => x.TenantId == tenantId && branchIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.NameEn, cancellationToken);

        foreach (var row in rows)
            row.BranchName = names.GetValueOrDefault(row.BranchId) ?? $"Unavailable branch ({row.BranchId})";

        return new PagedResponse<IEnumerable<BranchSalesReportDto>>(
            rows, parameter.PageNumber, parameter.PageSize, totalCount);
    }

    public async Task<PagedResponse<IEnumerable<CashierSalesReportDto>>> GetCashierSalesReportAsync(
        Guid tenantId, GetCashierSalesReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        var query = BuildCashierSalesReportQuery(tenantId, parameter);

        var offset = (parameter.PageNumber - 1) * parameter.PageSize;

        var rows = await query.ApplyOrder(parameter)
            .Skip(offset).Take(parameter.PageSize)
            .ToListAsync(cancellationToken);

        var totalCountIsKnown =
            (rows.Count > 0 && rows.Count < parameter.PageSize) ||   
            (rows.Count == 0 && offset == 0);                        

        var totalCount = totalCountIsKnown
            ? offset + rows.Count
            : await query.CountAsync(cancellationToken);

        return new PagedResponse<IEnumerable<CashierSalesReportDto>>(
            rows,
            parameter.PageNumber,
            parameter.PageSize,
            totalCount);
    }

    public async Task<PagedResponse<IEnumerable<PaymentMethodReportDto>>> GetPaymentMethodReportAsync(
        Guid tenantId, GetPaymentMethodReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        var query = BuildPaymentMethodReportQuery(tenantId, parameter.Filter);

        var offset = ((long)parameter.PageNumber - 1) * parameter.PageSize;

        // Skip accepts int; an offset beyond that cannot contain rows under our int count contract.
        if (offset > int.MaxValue)
            return new PagedResponse<IEnumerable<PaymentMethodReportDto>>(
                Array.Empty<PaymentMethodReportDto>(), parameter.PageNumber, parameter.PageSize,
                await query.CountAsync(cancellationToken));

        var rows = await query
            .ApplyOrder(parameter)
            .Skip((int)offset)
            .Take(parameter.PageSize)
            .ToListAsync(cancellationToken);

        var totalCountIsKnown = (rows.Count > 0 && rows.Count < parameter.PageSize) ||
            (rows.Count == 0 && offset == 0);

        var totalCount = totalCountIsKnown
            ? checked((int)(offset + rows.Count))
            : await query.CountAsync(cancellationToken);

        return new PagedResponse<IEnumerable<PaymentMethodReportDto>>(
            rows, parameter.PageNumber, parameter.PageSize, totalCount);
    }

    public async Task<PagedResponse<IEnumerable<ReturnsRefundsReportDto>>> GetReturnsRefundsReportAsync(
        Guid tenantId, GetReturnsRefundsReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        var query = BuildReturnsRefundsReportQuery(tenantId, parameter.Filter)
            .ApplyRefundStateFilter(parameter.Filter);

        var offset = ((long)parameter.PageNumber - 1) * parameter.PageSize;

        if (offset > int.MaxValue)
            return new PagedResponse<IEnumerable<ReturnsRefundsReportDto>>(
                Array.Empty<ReturnsRefundsReportDto>(), parameter.PageNumber, parameter.PageSize,
                await query.CountAsync(cancellationToken));

        var rows = await query.ApplyOrder(parameter).Skip((int)offset)
            .Take(parameter.PageSize).ToListAsync(cancellationToken);

        // A short nonempty page or empty first page reveals the total.
        var totalCountIsKnown = (rows.Count > 0 && rows.Count < parameter.PageSize) ||
            (rows.Count == 0 && offset == 0);

        var totalCount = totalCountIsKnown
            ? checked((int)(offset + rows.Count)) : await query.CountAsync(cancellationToken);

        if (rows.Count > 0)
        {
            var branchIds = rows.Select(x => x.BranchId).Distinct().ToList();
            var names = await _context.Branches
                .Where(x => x.TenantId == tenantId && branchIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.NameEn, cancellationToken);
            foreach (var row in rows)
                row.BranchName = names.GetValueOrDefault(row.BranchId) ?? $"Unavailable branch ({row.BranchId})";
        }

        return new PagedResponse<IEnumerable<ReturnsRefundsReportDto>>(
            rows, parameter.PageNumber, parameter.PageSize, totalCount);
    }

    public async Task<PagedResponse<IEnumerable<ShiftReconciliationReportDto>>> GetShiftReconciliationReportAsync(
    Guid tenantId,
    GetShiftReconciliationReportQueryParameter parameter,
    CancellationToken cancellationToken)
    {
        var query = BuildShiftReconciliationReportQuery(tenantId, parameter);

        var offset = ((long)parameter.PageNumber - 1) * parameter.PageSize;

        if (offset > int.MaxValue)
        {
            return new PagedResponse<IEnumerable<ShiftReconciliationReportDto>>(
                Array.Empty<ShiftReconciliationReportDto>(),
                parameter.PageNumber,
                parameter.PageSize,
                await query.CountAsync(cancellationToken));
        }

        var rows = await query.ApplyOrder(parameter)
            .Skip((int)offset)
            .Take(parameter.PageSize)
            .ToListAsync(cancellationToken);

        // A nonempty short page or empty first page reveals the total.
        var totalCountIsKnown =
            (rows.Count > 0 && rows.Count < parameter.PageSize) ||
            (rows.Count == 0 && offset == 0);

        var totalCount = totalCountIsKnown
            ? checked((int)(offset + rows.Count))
            : await query.CountAsync(cancellationToken);

        // Fetch names only for branches present on this page.
        if (rows.Count > 0)
        {
            var branchIds = rows
                .Select(row => row.BranchId)
                .Distinct()
                .ToList();

            var branchNames = await _context.Branches
                .Where(branch =>
                    branch.TenantId == tenantId &&
                    branchIds.Contains(branch.Id))
                .ToDictionaryAsync(
                    branch => branch.Id,
                    branch => branch.NameEn,
                    cancellationToken);

            foreach (var row in rows)
            {
                row.BranchName = branchNames.GetValueOrDefault(row.BranchId)
                    ?? $"Unavailable branch ({row.BranchId})";
            }
        }

        return new PagedResponse<IEnumerable<ShiftReconciliationReportDto>>(
            rows,
            parameter.PageNumber,
            parameter.PageSize,
            totalCount);
    }


    // Helpers to build the reports 

    private IQueryable<BranchSalesReportDto> BuildBranchSalesReportQuery(Guid tenantId, SalesReportFilter filter)
    {
        var sales = _context.Sales.ApplySalesFilter(tenantId, filter)
            .GroupBy(x => x.BranchId)
            .Select(group => new
            {
                BranchId = group.Key,
                Count = group.Count(),
                Total = group.Sum(x => x.Total),
                Discount = group.Sum(x => x.DiscountTotal),
                Tax = group.Sum(x => x.TaxTotal),
                Returns = 0m
            });
        // Returns belong to their receiving branch and their own completion period.
        var returns = _context.Returns.ApplyReturnsFilter(_context.Sales, tenantId, filter)
            .GroupBy(x => x.BranchId)
            .Select(group => new
            {
                BranchId = group.Key,
                Count = 0,
                Total = 0m,
                Discount = 0m,
                Tax = 0m,
                Returns = group.Sum(x => x.RefundAmount)
            });

        return sales.Concat(returns).GroupBy(x => x.BranchId)
            .Select(group => new BranchSalesReportDto
            {
                BranchId = group.Key,
                CurrencyCode = filter.CurrencyCode ?? "EGP",
                CompletedSaleCount = group.Sum(x => x.Count),
                SalesTotal = group.Sum(x => x.Total),
                DiscountTotal = group.Sum(x => x.Discount),
                SalesTaxTotal = group.Sum(x => x.Tax),
                AcceptedReturnAmount = group.Sum(x => x.Returns),
                SalesTotalAfterReturns = group.Sum(x => x.Total) - group.Sum(x => x.Returns)
            });
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

    private IQueryable<PaymentMethodReportDto> BuildPaymentMethodReportQuery(Guid tenantId, SalesReportFilter filter)
    {
        // Collections use payment time, even if the sale was completed outside the requested period.
        var payments =
            from payment in _context.SalePayments.ApplyPaymentFilter(tenantId, filter)
            join sale in _context.Sales.Where(x => x.TenantId == tenantId &&
                x.CurrencyCode == filter.CurrencyCode &&
                (!filter.BranchId.HasValue || x.BranchId == filter.BranchId.Value))
                on new { payment.TenantId, Id = payment.SaleId }
                equals new { sale.TenantId, sale.Id }
            select new
            {
                payment.PaymentMethodId,
                Name = payment.PaymentMethodNameSnapshot,
                Count = 1,
                Collected = payment.Amount - payment.ChangeAmount,
                Refunded = 0m
            };

        // Refund currency comes from the original sale; branch comes from the shift paying the refund.
        var refunds =
            from payment in _context.RefundPayments.ApplyPaymentFilter(tenantId, filter)
            join returned in _context.Returns.Where(x => x.TenantId == tenantId)
                on new { payment.TenantId, Id = payment.ReturnId } equals new { returned.TenantId, returned.Id }
            join sale in _context.Sales.Where(x => x.TenantId == tenantId && x.CurrencyCode == filter.CurrencyCode)
                on new { returned.TenantId, Id = returned.OriginalSaleId } equals new { sale.TenantId, sale.Id }
            join shift in _context.CashierShifts.Where(x => x.TenantId == tenantId &&
                (!filter.BranchId.HasValue || x.BranchId == filter.BranchId.Value))
                on new { payment.TenantId, Id = payment.ShiftId } equals new { shift.TenantId, shift.Id }
            select new
            {
                payment.PaymentMethodId,
                Name = payment.PaymentMethodNameSnapshot,
                Count = 0,
                Collected = 0m,
                Refunded = payment.Amount
            };

        return payments.Concat(refunds)
            .GroupBy(x => x.PaymentMethodId)
            .Select(group => new PaymentMethodReportDto
            {
                PaymentMethodId = group.Key,
                PaymentMethodName = group.Max(x => x.Name)!,
                CurrencyCode = filter.CurrencyCode ?? "EGP",
                CompletedPaymentCount = group.Sum(x => x.Count),
                CollectedAmount = group.Sum(x => x.Collected),
                RefundAmount = group.Sum(x => x.Refunded),
                NetCollectedAmount = group.Sum(x => x.Collected) - group.Sum(x => x.Refunded)
            });
    }
    
    private IQueryable<ReturnsRefundsReportDto> BuildReturnsRefundsReportQuery(
        Guid tenantId, SalesReportFilter filter)
    {
        // Select by return date; refund payments deliberately have no date or paying-branch filter.
        var returns = _context.Returns.ApplyReturnsFilter(_context.Sales, tenantId, filter);

        return
            from returned in returns
            join sale in _context.Sales.Where(x => x.TenantId == tenantId)
                on new { returned.TenantId, Id = returned.OriginalSaleId }
                equals new { sale.TenantId, sale.Id }
            let refundedAmount = _context.RefundPayments
                .Where(x => x.TenantId == tenantId && x.ReturnId == returned.Id &&
                    x.Status == PaymentStatuses.Completed)
                .Sum(x => (decimal?)x.Amount) ?? 0m
            select new ReturnsRefundsReportDto
            {
                ReturnId = returned.Id,
                ReturnNumber = returned.ReturnNumber,
                OriginalSaleId = returned.OriginalSaleId,
                OriginalReceiptNumber = sale.ReceiptNumber,
                CompletedAt = returned.CompletedAt,
                BranchId = returned.BranchId,
                ProcessedByUserId = returned.ProcessedByUserId,
                CurrencyCode = sale.CurrencyCode,
                AuthorizedRefundAmount = returned.RefundAmount,
                RefundedAmount = refundedAmount,
                RemainingRefundAmount = returned.RefundAmount - refundedAmount,
                // A zero-value return has no outstanding refund.
                RefundState = refundedAmount >= returned.RefundAmount ? RefundStates.FullyRefunded
                    : refundedAmount == 0m ? RefundStates.Unpaid : RefundStates.PartiallyRefunded
            };
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
    
    private IQueryable<CashierSalesReportDto> BuildCashierSalesReportQuery(
        Guid tenantId, GetCashierSalesReportQueryParameter parameter)
    {


        var sales = _context.Sales.ApplySalesFilter(tenantId, parameter.Filter);

        var shifts = _context.CashierShifts.ApplyCashierFilter(tenantId, parameter.Filter);

        return
            from sale in sales
            join shift in shifts
            on new { sale.TenantId, ShiftId = sale.ShiftId }
            equals new { shift.TenantId, ShiftId = shift.Id }
            group sale by shift.CashierUserId into cashier
            select new CashierSalesReportDto
            {
                CashierUserId = cashier.Key,
                CurrencyCode = parameter.Filter.CurrencyCode ?? "EGP",
                CompletedSaleCount = cashier.Count(),
                SalesTotal = cashier.Sum(s => s.Total),
                DiscountTotal = cashier.Sum(s => s.DiscountTotal),
                SalesTaxTotal = cashier.Sum(s => s.TaxTotal)
            };

    }
    private IQueryable<ShiftReconciliationReportDto> BuildShiftReconciliationReportQuery(
        Guid tenantId, GetShiftReconciliationReportQueryParameter parameter)
    {

        var shifts = _context.CashierShifts
            .ApplyShiftReportFilter(tenantId, parameter.Filter);

        return
            from shift in shifts

            // All completed cash payments belonging to this shift.
            let cashCollected =
                (from payment in _context.SalePayments
                 join sale in _context.Sales
                     on new { payment.TenantId, Id = payment.SaleId }
                     equals new { sale.TenantId, sale.Id }
                 where payment.TenantId == tenantId &&
                       sale.ShiftId == shift.Id &&
                       payment.Status == PaymentStatuses.Completed &&
                       payment.IsCashSnapshot
                 select (decimal?)(payment.Amount - payment.ChangeAmount))
                .Sum() ?? 0m

            // Refunds belong to the shift that paid them.
            let cashRefunded = _context.RefundPayments
                .Where(payment =>
                    payment.TenantId == tenantId &&
                    payment.ShiftId == shift.Id &&
                    payment.Status == PaymentStatuses.Completed &&
                    payment.IsCashSnapshot)
                .Sum(payment => (decimal?)payment.Amount) ?? 0m

            select new ShiftReconciliationReportDto
            {
                ShiftId = shift.Id,
                BranchId = shift.BranchId,
                TerminalId = shift.TerminalId,
                CashierUserId = shift.CashierUserId,
                OpenedAt = shift.OpenedAt,
                ClosedAt = shift.ClosedAt,
                Status = shift.Status,
                OpeningCash = shift.OpeningCash,
                CashCollected = cashCollected,
                CashRefunded = cashRefunded,

                ExpectedCash = shift.Status == ShiftReportStatuses.Closed
                    ? shift.ExpectedCash.Value
                    : shift.OpeningCash + cashCollected - cashRefunded,

                ClosingCash = shift.Status == ShiftReportStatuses.Closed
                    ? shift.ClosingCash
                    : null,

                Difference = shift.Status == ShiftReportStatuses.Closed
                    ? shift.Difference
                    : null
            };

    }
    public async Task<IReadOnlyList<ProductSalesReportDto>> GetProductSalesForExportAsync(
        Guid tenantId, GetProductSalesReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken)
    {
        var sales = BuildProductSalesQuery(tenantId, parameter.Filter);
        var returns = BuildProductReturnsQuery(tenantId, parameter.Filter);
        var query = BuildProductReportQuery(sales.Concat(returns), parameter.Filter.CurrencyCode ?? "EGP");
        var rows = await query.ApplyOrder(parameter).Take(checked(maxRows + 1)).ToListAsync(cancellationToken);
        if (rows.Count > maxRows)
            return rows;
        foreach (var batch in rows.Chunk(50))
            await AddProductNamesAsync(tenantId, batch.ToList(), cancellationToken);

        return rows;
    }
    public async Task<IReadOnlyList<BranchSalesReportDto>> GetBranchSalesForExportAsync(
        Guid tenantId, GetBranchSalesReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken)
    {
        var query = BuildBranchSalesReportQuery(tenantId, parameter.Filter);
        var rows = await query.ApplyOrder(parameter).Take(checked(maxRows + 1)).ToListAsync(cancellationToken);
        if (rows.Count > maxRows)
            return rows;
        foreach (var batch in rows.Chunk(50))
        {
            var ids = batch.Select(x => x.BranchId).Distinct().ToList();
            var names = await _context.Branches.Where(x => x.TenantId == tenantId && ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.NameEn, cancellationToken);
            foreach (var row in batch)
                row.BranchName = names.GetValueOrDefault(row.BranchId) ?? $"Unavailable branch ({row.BranchId})";
        }

        return rows;
    }
    public async Task<IReadOnlyList<CashierSalesReportDto>> GetCashierSalesForExportAsync(
        Guid tenantId, GetCashierSalesReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken)
    {
        var query = BuildCashierSalesReportQuery(tenantId, parameter);
        var rows = await query.ApplyOrder(parameter).Take(checked(maxRows + 1)).ToListAsync(cancellationToken);
        if (rows.Count > maxRows)
            return rows;

        return rows;
    }
    public async Task<IReadOnlyList<PaymentMethodReportDto>> GetPaymentMethodForExportAsync(
        Guid tenantId, GetPaymentMethodReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken)
    {
        var query = BuildPaymentMethodReportQuery(tenantId, parameter.Filter);
        var rows = await query.ApplyOrder(parameter).Take(checked(maxRows + 1)).ToListAsync(cancellationToken);
        if (rows.Count > maxRows)
            return rows;

        return rows;
    }
    public async Task<IReadOnlyList<ReturnsRefundsReportDto>> GetReturnsRefundsForExportAsync(
        Guid tenantId, GetReturnsRefundsReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken)
    {
        var query = BuildReturnsRefundsReportQuery(tenantId, parameter.Filter).ApplyRefundStateFilter(parameter.Filter);
        var rows = await query.ApplyOrder(parameter).Take(checked(maxRows + 1)).ToListAsync(cancellationToken);
        if (rows.Count > maxRows)
            return rows;
        foreach (var batch in rows.Chunk(50))
        {
            var ids = batch.Select(x => x.BranchId).Distinct().ToList();
            var names = await _context.Branches.Where(x => x.TenantId == tenantId && ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.NameEn, cancellationToken);
            foreach (var row in batch)
                row.BranchName = names.GetValueOrDefault(row.BranchId) ?? $"Unavailable branch ({row.BranchId})";
        }

        return rows;
    }
    public async Task<IReadOnlyList<ShiftReconciliationReportDto>> GetShiftReconciliationForExportAsync(
        Guid tenantId, GetShiftReconciliationReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken)
    {
        var query = BuildShiftReconciliationReportQuery(tenantId, parameter);
        var rows = await query.ApplyOrder(parameter).Take(checked(maxRows + 1)).ToListAsync(cancellationToken);
        if (rows.Count > maxRows)
            return rows;
        foreach (var batch in rows.Chunk(50))
        {
            var ids = batch.Select(x => x.BranchId).Distinct().ToList();
            var names = await _context.Branches.Where(x => x.TenantId == tenantId && ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.NameEn, cancellationToken);
            foreach (var row in batch)
                row.BranchName = names.GetValueOrDefault(row.BranchId) ?? $"Unavailable branch ({row.BranchId})";
        }

        return rows;
    }
}
