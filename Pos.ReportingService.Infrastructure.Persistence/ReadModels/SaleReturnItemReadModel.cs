namespace Pos.ReportingService.Infrastructure.Persistence.ReadModels;

/// <summary>Read-only reporting projection of Sales records.</summary>
public class SaleReturnItemReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ReturnId { get; set; }
    public Guid OriginalSaleId { get; set; }
    public Guid OriginalSaleItemId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public bool Restock { get; set; }
    public string StockCondition { get; set; } = string.Empty;
}
