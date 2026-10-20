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
        RuleFor(x => x.FromUtc).NotEmpty();
        RuleFor(x => x.ToUtcExclusive).NotEmpty().GreaterThan(x => x.FromUtc);
        RuleFor(x => x.CurrencyCode).NotEmpty().Matches("^[A-Z]{3}$")
            .WithMessage("CurrencyCode must contain three uppercase letters.");
    }
}
