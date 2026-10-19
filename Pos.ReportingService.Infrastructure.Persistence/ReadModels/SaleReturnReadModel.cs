namespace Pos.ReportingService.Infrastructure.Persistence.ReadModels;

/// <summary>Read-only reporting projection of Sales records.</summary>
public class SaleReturnReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid OriginalSaleId { get; set; }
    public string? ReturnNumber { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public Guid ProcessedByUserId { get; set; }
    public DateTime CompletedAt { get; set; }
}
