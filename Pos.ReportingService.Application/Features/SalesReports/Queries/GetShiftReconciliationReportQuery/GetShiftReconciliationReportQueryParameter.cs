using Pos.SalesService.Application.Parameters;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;

public class GetShiftReconciliationReportQueryParameter : RequestParameter<ShiftReconciliationOrderKey>
{
    public GetShiftReconciliationReportQueryParameter()
    {
        OrderKey = ShiftReconciliationOrderKey.OpenedAt;
        OrderDescending = true;
    }
    public ShiftReconciliationReportFilter Filter { get; set; } = new();
}

public class ShiftReconciliationReportFilter
{
    public Guid? BranchId { get; set; }
    public Guid? TerminalId { get; set; }
    public Guid? CashierUserId { get; set; }
    // Date limits select shifts by opening time, not individual payment time.
    public DateTimeOffset? FromUtc { get; set; }
    public DateTimeOffset? ToUtcExclusive { get; set; }
    public string? Status { get; set; }
}

public enum ShiftReconciliationOrderKey
{
    OpenedAt,
    ClosedAt,
    ExpectedCash,
    ClosingCash,
    Difference
}

public static class ShiftReportStatuses
{
    public const string Open = "Open";
    public const string Closed = "Closed";
}
