namespace Pos.ReportingService.Application.Features.InventoryReports.DTOs;

public class CurrentInventoryReportDto
{
    public Guid StockBalanceId { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid? ProductVariantId { get; set; }
    public string? VariantName { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal LowStockThreshold { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
