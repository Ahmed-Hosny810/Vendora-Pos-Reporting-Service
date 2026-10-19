namespace Pos.ReportingService.Domain.Models;

/// <summary>Daily stock quantities captured by Reporting for historical comparisons.</summary>
public class InventorySnapshot
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    // Business date in the tenant's reporting timezone.
    public DateOnly SnapshotDate { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal LowStockThreshold { get; set; }
    // Actual UTC capture time;
    public DateTime CapturedAt { get; set; }
}
