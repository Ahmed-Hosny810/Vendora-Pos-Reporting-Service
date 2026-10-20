namespace Pos.ReportingService.Application.Features.SalesReports.Parameters;

public class SalesReportFilter
{
    public Guid? BranchId { get; set; }
    public DateTimeOffset FromUtc { get; set; }
    public DateTimeOffset ToUtcExclusive { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
}
