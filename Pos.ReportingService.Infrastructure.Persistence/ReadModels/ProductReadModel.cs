namespace Pos.ReportingService.Infrastructure.Persistence.ReadModels;

public class ProductReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string NameEn { get; set; } = string.Empty;
}
