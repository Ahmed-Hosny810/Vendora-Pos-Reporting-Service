using Pos.ReportingService.Application.Features.InventoryReports.DTOs;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

public static class InventoryReportQueryExtensions
{
    public static IQueryable<StockBalanceReadModel> ApplyInventoryFilter(
        this IQueryable<StockBalanceReadModel> query, Guid tenantId, CurrentInventoryReportFilter filter)
    {
        return query.Where(x => x.TenantId == tenantId &&
            (!filter.BranchId.HasValue || x.BranchId == filter.BranchId.Value) &&
            (!filter.ProductId.HasValue || x.ProductId == filter.ProductId.Value) &&
            (!filter.ProductVariantId.HasValue || x.ProductVariantId == filter.ProductVariantId.Value));
    }

    public static IOrderedQueryable<CurrentInventoryReportDto> ApplyOrder(
        this IQueryable<CurrentInventoryReportDto> query, GetCurrentInventoryReportQueryParameter parameter)
    {
        var ordered = parameter.OrderKey switch
        {
            CurrentInventoryOrderKey.QuantityAvailable => parameter.OrderDescending
                ? query.OrderByDescending(x => x.QuantityAvailable) : query.OrderBy(x => x.QuantityAvailable),
            CurrentInventoryOrderKey.QuantityOnHand => parameter.OrderDescending
                ? query.OrderByDescending(x => x.QuantityOnHand) : query.OrderBy(x => x.QuantityOnHand),
            CurrentInventoryOrderKey.QuantityReserved => parameter.OrderDescending
                ? query.OrderByDescending(x => x.QuantityReserved) : query.OrderBy(x => x.QuantityReserved),
            CurrentInventoryOrderKey.LowStockThreshold => parameter.OrderDescending
                ? query.OrderByDescending(x => x.LowStockThreshold) : query.OrderBy(x => x.LowStockThreshold),
            CurrentInventoryOrderKey.UpdatedAt => parameter.OrderDescending
                ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter.OrderKey))
        };
        return ordered.ThenBy(x => x.StockBalanceId);
    }
}
