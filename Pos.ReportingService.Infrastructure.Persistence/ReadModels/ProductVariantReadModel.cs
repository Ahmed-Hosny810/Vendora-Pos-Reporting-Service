namespace Pos.ReportingService.Infrastructure.Persistence.ReadModels;

public class ProductVariantReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
}
