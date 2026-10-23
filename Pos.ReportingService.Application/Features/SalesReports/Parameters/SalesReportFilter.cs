namespace Pos.ReportingService.Application.Features.SalesReports.Parameters;

public class SalesReportFilter
{
    public Guid? BranchId { get; set; }
    public DateTimeOffset? FromUtc { get; set; }
    public DateTimeOffset? ToUtcExclusive { get; set; }
    private string? _currencyCode;

    // Omitted or empty currency uses the default; never combine currencies in a monetary total.
    public string? CurrencyCode
    {
        get => _currencyCode ?? "EGP";
        set => _currencyCode = string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
