using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Domain.Models;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class InventorySnapshotConfiguration : IEntityTypeConfiguration<InventorySnapshot>
{
    public void Configure(EntityTypeBuilder<InventorySnapshot> builder)
    {
        builder.ToTable("InventorySnapshots", "reporting");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SnapshotDate).HasColumnType("date");
        builder.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
        builder.Property(x => x.QuantityReserved).HasPrecision(18, 3);
        builder.Property(x => x.LowStockThreshold).HasPrecision(18, 3);
        builder.Property(x => x.CapturedAt).HasColumnType("datetime2");

        // Include products without a variant in the uniqueness guarantee as well.
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.SnapshotDate, x.ProductId, x.ProductVariantId })
            .IsUnique().HasFilter(null);
        builder.HasIndex(x => new { x.TenantId, x.SnapshotDate });
        // External IDs are references, not cross-schema foreign keys owned by Reporting.
    }
}
