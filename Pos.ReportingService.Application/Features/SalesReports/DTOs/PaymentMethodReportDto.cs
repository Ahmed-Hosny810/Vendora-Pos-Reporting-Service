namespace Pos.ReportingService.Application.Features.SalesReports.DTOs;

public class PaymentMethodReportDto
{
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public int CompletedPaymentCount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal NetCollectedAmount { get; set; }
}
