using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

public static class ShiftReconciliationReportQueryExtensions
{
    public static IQueryable<CashierShiftReadModel> ApplyShiftReportFilter(
        this IQueryable<CashierShiftReadModel> query, Guid tenantId, ShiftReconciliationReportFilter filter)
    {
        var from = filter.FromUtc?.UtcDateTime;
        var to = filter.ToUtcExclusive?.UtcDateTime;
        return query.Where(x => x.TenantId == tenantId &&
            (!filter.BranchId.HasValue || x.BranchId == filter.BranchId.Value) &&
            (!filter.TerminalId.HasValue || x.TerminalId == filter.TerminalId.Value) &&
            (!filter.CashierUserId.HasValue || x.CashierUserId == filter.CashierUserId.Value) &&
            (filter.Status == null || x.Status == filter.Status) &&
            (!from.HasValue || x.OpenedAt >= from.Value) &&
            (!to.HasValue || x.OpenedAt < to.Value));
    }

    public static IOrderedQueryable<ShiftReconciliationReportDto> ApplyOrder(
        this IQueryable<ShiftReconciliationReportDto> query, GetShiftReconciliationReportQueryParameter parameter)
    {
        var ordered = parameter.OrderKey switch
        {
            ShiftReconciliationOrderKey.OpenedAt => parameter.OrderDescending
                ? query.OrderByDescending(x => x.OpenedAt) : query.OrderBy(x => x.OpenedAt),
            ShiftReconciliationOrderKey.ClosedAt => parameter.OrderDescending
                ? query.OrderByDescending(x => x.ClosedAt) : query.OrderBy(x => x.ClosedAt),
            ShiftReconciliationOrderKey.ExpectedCash => parameter.OrderDescending
                ? query.OrderByDescending(x => x.ExpectedCash) : query.OrderBy(x => x.ExpectedCash),
            ShiftReconciliationOrderKey.ClosingCash => parameter.OrderDescending
                ? query.OrderByDescending(x => x.ClosingCash) : query.OrderBy(x => x.ClosingCash),
            ShiftReconciliationOrderKey.Difference => parameter.OrderDescending
                ? query.OrderByDescending(x => x.Difference) : query.OrderBy(x => x.Difference),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter.OrderKey))
        };
        return ordered.ThenBy(x => x.ShiftId);
    }
}
