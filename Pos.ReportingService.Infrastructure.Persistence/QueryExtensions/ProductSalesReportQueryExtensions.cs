using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

public static class ProductSalesReportQueryExtensions
{
    public static IQueryable<SaleItemReadModel> ApplyProductFilter(this IQueryable<SaleItemReadModel> query,
        Guid tenantId, ProductSalesReportFilter filter)
    {
        return query.Where(x => x.TenantId == tenantId &&
            (!filter.ProductId.HasValue || x.ProductId == filter.ProductId.Value) &&
            (!filter.ProductVariantId.HasValue || x.ProductVariantId == filter.ProductVariantId.Value));
    }

    public static IQueryable<SaleReturnItemReadModel> ApplyProductFilter(this IQueryable<SaleReturnItemReadModel> query,
        Guid tenantId, ProductSalesReportFilter filter)
    {
        return query.Where(x => x.TenantId == tenantId &&
            (!filter.ProductId.HasValue || x.ProductId == filter.ProductId.Value) &&
            (!filter.ProductVariantId.HasValue || x.ProductVariantId == filter.ProductVariantId.Value));
    }

    public static IOrderedQueryable<ProductSalesReportDto> ApplyOrder(this IQueryable<ProductSalesReportDto> query,
        GetProductSalesReportQueryParameter parameter)
    {
        var ordered = parameter.OrderKey switch
        {
            ProductSalesOrderKey.QuantitySold => parameter.OrderDescending
                ? query.OrderByDescending(x => x.QuantitySold) : query.OrderBy(x => x.QuantitySold),
            ProductSalesOrderKey.SalesTotal => parameter.OrderDescending
                ? query.OrderByDescending(x => x.SalesTotal) : query.OrderBy(x => x.SalesTotal),
            ProductSalesOrderKey.SalesTotalAfterReturns => parameter.OrderDescending
                ? query.OrderByDescending(x => x.SalesTotalAfterReturns) : query.OrderBy(x => x.SalesTotalAfterReturns),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter.OrderKey))
        };
        return ordered.ThenBy(x => x.ProductId).ThenBy(x => x.ProductVariantId);
    }
}
