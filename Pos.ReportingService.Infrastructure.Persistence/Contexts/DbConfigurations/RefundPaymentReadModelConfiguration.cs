using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class RefundPaymentReadModelConfiguration : IEntityTypeConfiguration<RefundPaymentReadModel>
{
    public void Configure(EntityTypeBuilder<RefundPaymentReadModel> builder)
    {
        // Read the existing source table; Reporting migrations must not create or alter it.
        builder.HasNoKey();
        builder.ToView("RefundPayments", "sales");
        builder.Property(x => x.Amount).HasPrecision(18, 2);
    }
}
