using FluentValidation;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;

public class GetSalesDashboardQueryValidator : AbstractValidator<GetSalesDashboardQuery>
{
    public GetSalesDashboardQueryValidator()
    {
        RuleFor(x => x.Filter).NotNull().SetValidator(new SalesReportFilterValidator());
    }
}

public class SalesReportFilterValidator : AbstractValidator<SalesReportFilter>
{
    public SalesReportFilterValidator()
    {
        RuleFor(x => x.BranchId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        RuleFor(x => x.ToUtcExclusive)
            .Must((filter, to) => !filter.FromUtc.HasValue || !to.HasValue || to.Value > filter.FromUtc.Value)
            .WithMessage("ToUtcExclusive must be after FromUtc when both are supplied.");
        RuleFor(x => x.CurrencyCode).NotEmpty().Matches("^[A-Z]{3}$")
            .WithMessage("CurrencyCode must contain three uppercase letters.");
    }
}
