using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class SaleReadModelConfiguration : IEntityTypeConfiguration<SaleReadModel>
{
    public void Configure(EntityTypeBuilder<SaleReadModel> builder)
    {
        // Read the existing source table; Reporting migrations must not create or alter it.
        builder.HasNoKey();
        builder.ToView("Sales", "sales");
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.DiscountTotal).HasPrecision(18, 2);
        builder.Property(x => x.TaxTotal).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.Property(x => x.ChangeAmount).HasPrecision(18, 2);
    }
}
