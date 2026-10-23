using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels.Constants;

namespace Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

public static class PaymentMethodReportQueryExtensions
{
    public static IQueryable<ReadModels.SalePaymentReadModel> ApplyPaymentFilter(
        this IQueryable<ReadModels.SalePaymentReadModel> query, Guid tenantId,
        Application.Features.SalesReports.Parameters.SalesReportFilter filter)
    {
        var from = filter.FromUtc?.UtcDateTime;
        var to = filter.ToUtcExclusive?.UtcDateTime;
        return query.Where(x => x.TenantId == tenantId &&
            x.Status == PaymentStatuses.Completed &&
            (!from.HasValue || x.PaidAt >= from.Value) && (!to.HasValue || x.PaidAt < to.Value));
    }

    public static IQueryable<ReadModels.RefundPaymentReadModel> ApplyPaymentFilter(
        this IQueryable<ReadModels.RefundPaymentReadModel> query, Guid tenantId,
        Application.Features.SalesReports.Parameters.SalesReportFilter filter)
    {
        var from = filter.FromUtc?.UtcDateTime;
        var to = filter.ToUtcExclusive?.UtcDateTime;
        return query.Where(x => x.TenantId == tenantId &&
            x.Status == ReadModels.Constants.PaymentStatuses.Completed &&
            (!from.HasValue || x.PaidAt >= from.Value) && (!to.HasValue || x.PaidAt < to.Value));
    }

    public static IOrderedQueryable<PaymentMethodReportDto> ApplyOrder(this IQueryable<PaymentMethodReportDto> query,
        GetPaymentMethodReportQueryParameter parameter)
    {
        var ordered = parameter.OrderKey switch
        {
            PaymentMethodOrderKey.CompletedPaymentCount => parameter.OrderDescending
                ? query.OrderByDescending(x => x.CompletedPaymentCount) : query.OrderBy(x => x.CompletedPaymentCount),
            PaymentMethodOrderKey.CollectedAmount => parameter.OrderDescending
                ? query.OrderByDescending(x => x.CollectedAmount) : query.OrderBy(x => x.CollectedAmount),
            PaymentMethodOrderKey.NetCollectedAmount => parameter.OrderDescending
                ? query.OrderByDescending(x => x.NetCollectedAmount) : query.OrderBy(x => x.NetCollectedAmount),
            PaymentMethodOrderKey.RefundAmount => parameter.OrderDescending
                ? query.OrderByDescending(x => x.RefundAmount) : query.OrderBy(x => x.RefundAmount),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter.OrderKey))
        };
        return ordered.ThenBy(x => x.PaymentMethodId);
    }
}
