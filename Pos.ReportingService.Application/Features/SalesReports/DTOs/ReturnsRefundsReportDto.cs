namespace Pos.ReportingService.Application.Features.SalesReports.DTOs;

public class ReturnsRefundsReportDto
{
    public Guid ReturnId { get; set; }
    public string? ReturnNumber { get; set; }
    public Guid OriginalSaleId { get; set; }
    public string? OriginalReceiptNumber { get; set; }
    public DateTime CompletedAt { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid ProcessedByUserId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal AuthorizedRefundAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal RemainingRefundAmount { get; set; }
    public string RefundState { get; set; } = string.Empty;
}
