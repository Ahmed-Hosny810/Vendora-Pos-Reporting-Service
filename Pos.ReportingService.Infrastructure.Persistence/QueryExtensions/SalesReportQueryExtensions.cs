using Pos.ReportingService.Infrastructure.Persistence.ReadModels.Constants;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

public static class SalesReportQueryExtensions
{
    public static IQueryable<SaleReadModel> ApplySalesFilter(this IQueryable<SaleReadModel> query, Guid tenantId, SalesReportFilter filter)
    {
        var from = filter.FromUtc.UtcDateTime;
        var to = filter.ToUtcExclusive.UtcDateTime;

        return query.Where(x => x.TenantId == tenantId &&
            (x.Status == SaleStatuses.Completed || x.Status == SaleStatuses.PartiallyReturned || x.Status == SaleStatuses.Returned) &&
            x.CompletedAt >= from && x.CompletedAt < to &&
            x.CurrencyCode == filter.CurrencyCode &&
            (!filter.BranchId.HasValue || x.BranchId == filter.BranchId.Value));
    }

    public static IQueryable<SaleReturnReadModel> ApplyReturnsFilter(
        this IQueryable<SaleReturnReadModel> query,
        IQueryable<SaleReadModel> sales,
        Guid tenantId,
        SalesReportFilter filter)
    {
        var from = filter.FromUtc.UtcDateTime;
        var to = filter.ToUtcExclusive.UtcDateTime;

        return query.Where(r =>r.TenantId == tenantId &&
            r.CompletedAt >= from && r.CompletedAt < to &&
            (!filter.BranchId.HasValue || r.BranchId == filter.BranchId.Value) &&
            sales.Any(s => s.Id == r.OriginalSaleId &&
                           s.TenantId == tenantId &&
                           s.CurrencyCode == filter.CurrencyCode));
    }
    
}
