namespace Pos.ReportingService.Infrastructure.Persistence.ReadModels;

/// <summary>Read-only reporting projection of Inventory records.</summary>
public class LowStockAlertReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal Threshold { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
