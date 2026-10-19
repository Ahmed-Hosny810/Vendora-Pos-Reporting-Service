using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class SaleItemReadModelConfiguration : IEntityTypeConfiguration<SaleItemReadModel>
{
    public void Configure(EntityTypeBuilder<SaleItemReadModel> builder)
    {
        // Read the existing source table; Reporting migrations must not create or alter it.
        builder.HasNoKey();
        builder.ToView("SaleItems", "sales");
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.UnitCost).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxRate).HasPrecision(5, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);
        builder.Property(x => x.ReturnedQuantity).HasPrecision(18, 3);
    }
}
