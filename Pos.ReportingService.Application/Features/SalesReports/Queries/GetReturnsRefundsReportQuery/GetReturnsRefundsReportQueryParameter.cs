using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.SalesService.Application.Parameters;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;

public class GetReturnsRefundsReportQueryParameter : RequestParameter<ReturnsRefundsOrderKey>
{
    public GetReturnsRefundsReportQueryParameter()
    {
        OrderKey = ReturnsRefundsOrderKey.CompletedAt;
        OrderDescending = true;
    }
    public ReturnsRefundsReportFilter Filter { get; set; } = new();
}

public class ReturnsRefundsReportFilter : SalesReportFilter
{
    public string? RefundState { get; set; }
}

public enum ReturnsRefundsOrderKey
{
    CompletedAt,
    AuthorizedRefundAmount,
    RefundedAmount,
    RemainingRefundAmount
}

public static class RefundStates
{
    public const string Unpaid = "Unpaid";
    public const string PartiallyRefunded = "PartiallyRefunded";
    public const string FullyRefunded = "FullyRefunded";
}
