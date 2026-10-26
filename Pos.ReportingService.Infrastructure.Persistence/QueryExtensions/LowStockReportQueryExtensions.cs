using Pos.ReportingService.Application.Features.InventoryReports.DTOs;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

public static class LowStockReportQueryExtensions
{
    public static IQueryable<StockBalanceReadModel> ApplyLowStockFilter(
        this IQueryable<StockBalanceReadModel> query, Guid tenantId, LowStockReportFilter filter)
    {
        return query.Where(x => x.TenantId == tenantId && x.QuantityOnHand <= x.LowStockThreshold &&
            (!filter.BranchId.HasValue || x.BranchId == filter.BranchId.Value) &&
            (!filter.ProductId.HasValue || x.ProductId == filter.ProductId.Value) &&
            (!filter.ProductVariantId.HasValue || x.ProductVariantId == filter.ProductVariantId.Value));
    }

    public static IOrderedQueryable<LowStockReportDto> ApplyOrder(
        this IQueryable<LowStockReportDto> query, GetLowStockReportQueryParameter parameter)
    {
        var ordered = parameter.OrderKey switch
        {
            LowStockOrderKey.QuantityAvailable => parameter.OrderDescending
                ? query.OrderByDescending(x => x.QuantityAvailable) : query.OrderBy(x => x.QuantityAvailable),
            LowStockOrderKey.QuantityOnHand => parameter.OrderDescending
                ? query.OrderByDescending(x => x.QuantityOnHand) : query.OrderBy(x => x.QuantityOnHand),
            LowStockOrderKey.QuantityReserved => parameter.OrderDescending
                ? query.OrderByDescending(x => x.QuantityReserved) : query.OrderBy(x => x.QuantityReserved),
            LowStockOrderKey.LowStockThreshold => parameter.OrderDescending
                ? query.OrderByDescending(x => x.LowStockThreshold) : query.OrderBy(x => x.LowStockThreshold),
            LowStockOrderKey.UpdatedAt => parameter.OrderDescending
                ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter.OrderKey))
        };
        return ordered.ThenBy(x => x.StockBalanceId);
    }
}
