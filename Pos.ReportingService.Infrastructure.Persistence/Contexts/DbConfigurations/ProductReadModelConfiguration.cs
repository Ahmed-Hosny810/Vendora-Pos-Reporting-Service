using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class ProductReadModelConfiguration : IEntityTypeConfiguration<ProductReadModel>
{
    public void Configure(EntityTypeBuilder<ProductReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("Products", "catalog");
    }
}
