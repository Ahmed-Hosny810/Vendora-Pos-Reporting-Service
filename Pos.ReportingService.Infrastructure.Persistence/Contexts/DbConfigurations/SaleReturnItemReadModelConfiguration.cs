using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class SaleReturnItemReadModelConfiguration : IEntityTypeConfiguration<SaleReturnItemReadModel>
{
    public void Configure(EntityTypeBuilder<SaleReturnItemReadModel> builder)
    {
        // Read the existing source table;
        builder.HasNoKey();
        builder.ToView("ReturnItems", "sales");
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.RefundAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
    }
}
