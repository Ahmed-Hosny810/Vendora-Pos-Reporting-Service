namespace Pos.ReportingService.Application.Features.SalesReports.Parameters;

public class DailySalesReportFilter
{
    public Guid? BranchId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDateExclusive { get; set; }
    private string? _currencyCode;
    public string? CurrencyCode
    {
        get => _currencyCode ?? "EGP";
        set => _currencyCode = string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
