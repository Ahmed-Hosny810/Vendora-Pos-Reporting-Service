namespace Pos.ReportingService.Infrastructure.Persistence.ReadModels;

/// <summary>Read-only reporting projection of Sales records.</summary>
public class SaleReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? ReceiptNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public bool PricesIncludeTax { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? CustomerNameSnapshot { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
