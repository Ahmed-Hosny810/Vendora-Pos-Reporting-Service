namespace Pos.ReportingService.Application.Features.InventoryReports.DTOs;

public class LowStockReportDto : CurrentInventoryReportDto
{
    public Guid? ActiveAlertId { get; set; }
    public DateTime? DetectedAt { get; set; }
}
