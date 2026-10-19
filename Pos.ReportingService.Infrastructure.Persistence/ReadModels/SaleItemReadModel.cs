namespace Pos.ReportingService.Infrastructure.Persistence.ReadModels;

/// <summary>Read-only reporting projection of Sales records.</summary>
public class SaleItemReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SaleId { get; set; }
    public int ItemNumber { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string? VariantNameSnapshot { get; set; }
    public string? SkuSnapshot { get; set; }
    public string? BarcodeSnapshot { get; set; }
    public string UnitNameSnapshot { get; set; } = string.Empty;
    public bool TrackInventorySnapshot { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReturnedQuantity { get; set; }
}
