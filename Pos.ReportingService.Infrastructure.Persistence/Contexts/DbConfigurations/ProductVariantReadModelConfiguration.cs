using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class ProductVariantReadModelConfiguration : IEntityTypeConfiguration<ProductVariantReadModel>
{
    public void Configure(EntityTypeBuilder<ProductVariantReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("ProductVariants", "catalog");
    }
}
