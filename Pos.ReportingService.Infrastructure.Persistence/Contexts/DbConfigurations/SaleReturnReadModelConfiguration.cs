using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class SaleReturnReadModelConfiguration : IEntityTypeConfiguration<SaleReturnReadModel>
{
    public void Configure(EntityTypeBuilder<SaleReturnReadModel> builder)
    {
        // Read the existing source table;
        builder.HasNoKey();
        builder.ToView("Returns", "sales");
        builder.Property(x => x.RefundAmount).HasPrecision(18, 2);
    }
}
