namespace Pos.ReportingService.Infrastructure.Persistence.ReadModels;

/// <summary>Read-only reporting projection of Sales records.</summary>
public class SalePaymentReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SaleId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodNameSnapshot { get; set; } = string.Empty;
    public string PaymentMethodCodeSnapshot { get; set; } = string.Empty;
    public bool IsCashSnapshot { get; set; }
    public decimal Amount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid ReceivedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
