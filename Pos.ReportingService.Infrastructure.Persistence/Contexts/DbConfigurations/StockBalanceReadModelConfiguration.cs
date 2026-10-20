using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class StockBalanceReadModelConfiguration : IEntityTypeConfiguration<StockBalanceReadModel>
{
    public void Configure(EntityTypeBuilder<StockBalanceReadModel> builder)
    {
        // Read the existing source table;
        builder.HasNoKey();
        builder.ToView("StockBalances", "inventory");
        builder.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
        builder.Property(x => x.QuantityReserved).HasPrecision(18, 3);
        builder.Property(x => x.LowStockThreshold).HasPrecision(18, 3);
    }
}
