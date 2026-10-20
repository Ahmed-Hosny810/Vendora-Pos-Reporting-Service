using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class SalePaymentReadModelConfiguration : IEntityTypeConfiguration<SalePaymentReadModel>
{
    public void Configure(EntityTypeBuilder<SalePaymentReadModel> builder)
    {
        // Read the existing source table; 
        builder.HasNoKey();
        builder.ToView("SalePayments", "sales");
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ChangeAmount).HasPrecision(18, 2);
    }
}
