using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class CashierShiftReadModelConfiguration : IEntityTypeConfiguration<CashierShiftReadModel>
{
    public void Configure(EntityTypeBuilder<CashierShiftReadModel> builder)
    {
        // Read the existing source table;
        builder.HasNoKey();
        builder.ToView("CashierShifts", "sales");
        builder.Property(x => x.OpeningCash).HasPrecision(18, 2);
        builder.Property(x => x.ClosingCash).HasPrecision(18, 2);
        builder.Property(x => x.ExpectedCash).HasPrecision(18, 2);
        builder.Property(x => x.Difference).HasPrecision(18, 2);
    }
}
