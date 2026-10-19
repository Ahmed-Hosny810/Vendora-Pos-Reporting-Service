using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class LowStockAlertReadModelConfiguration : IEntityTypeConfiguration<LowStockAlertReadModel>
{
    public void Configure(EntityTypeBuilder<LowStockAlertReadModel> builder)
    {
        // Read the existing source table; Reporting migrations must not create or alter it.
        builder.HasNoKey();
        builder.ToView("LowStockAlerts", "inventory");
        builder.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
        builder.Property(x => x.Threshold).HasPrecision(18, 3);
    }
}
