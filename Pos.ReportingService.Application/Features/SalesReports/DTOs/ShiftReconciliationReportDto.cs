namespace Pos.ReportingService.Application.Features.SalesReports.DTOs;

public class ShiftReconciliationReportDto
{
    public Guid ShiftId { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid TerminalId { get; set; }
    public Guid CashierUserId { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal OpeningCash { get; set; }
    public decimal CashCollected { get; set; }
    public decimal CashRefunded { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal? Difference { get; set; }
}
