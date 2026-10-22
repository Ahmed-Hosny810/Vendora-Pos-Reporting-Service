using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts.DbConfigurations;

public class BranchReadModelConfiguration : IEntityTypeConfiguration<BranchReadModel>
{
    public void Configure(EntityTypeBuilder<BranchReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("Branches", "branch");
    }
}
