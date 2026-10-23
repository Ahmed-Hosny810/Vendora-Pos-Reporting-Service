using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;

namespace Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

public static class ReturnsRefundsReportQueryExtensions
{
    public static IQueryable<ReturnsRefundsReportDto> ApplyRefundStateFilter(
        this IQueryable<ReturnsRefundsReportDto> query, ReturnsRefundsReportFilter filter)
    {
        return filter.RefundState == null ? query : query.Where(x => x.RefundState == filter.RefundState);
    }

    public static IOrderedQueryable<ReturnsRefundsReportDto> ApplyOrder(
        this IQueryable<ReturnsRefundsReportDto> query, GetReturnsRefundsReportQueryParameter parameter)
    {
        var ordered = parameter.OrderKey switch
        {
            ReturnsRefundsOrderKey.CompletedAt => parameter.OrderDescending
                ? query.OrderByDescending(x => x.CompletedAt) : query.OrderBy(x => x.CompletedAt),
            ReturnsRefundsOrderKey.AuthorizedRefundAmount => parameter.OrderDescending
                ? query.OrderByDescending(x => x.AuthorizedRefundAmount) : query.OrderBy(x => x.AuthorizedRefundAmount),
            ReturnsRefundsOrderKey.RefundedAmount => parameter.OrderDescending
                ? query.OrderByDescending(x => x.RefundedAmount) : query.OrderBy(x => x.RefundedAmount),
            ReturnsRefundsOrderKey.RemainingRefundAmount => parameter.OrderDescending
                ? query.OrderByDescending(x => x.RemainingRefundAmount) : query.OrderBy(x => x.RemainingRefundAmount),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter.OrderKey))
        };
        return ordered.ThenBy(x => x.ReturnId);
    }
}
