using FluentValidation;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetDailySalesQuery;

public class GetDailySalesReportQueryValidator : AbstractValidator<GetDailySalesReportQuery>
{
    public GetDailySalesReportQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull()
            .SetValidator(new DailySalesReportFilterValidator());
    }
}

public class DailySalesReportFilterValidator : AbstractValidator<Pos.ReportingService.Application.Features.SalesReports.Parameters.DailySalesReportFilter>
{
    public DailySalesReportFilterValidator()
    {
        RuleFor(x => x.BranchId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        // Leave room for UTC conversion at the lower DateTime boundary.
        RuleFor(x => x.FromDate).Must(date => !date.HasValue || date.Value.Year >= 2);
        RuleFor(x => x.ToDateExclusive).Must(date => !date.HasValue || date.Value.Year >= 2);
        RuleFor(x => x.ToDateExclusive)
            .Must((filter, to) => !filter.FromDate.HasValue || !to.HasValue || to.Value > filter.FromDate.Value)
            .WithMessage("ToDateExclusive must be after FromDate when both are supplied.");
        RuleFor(x => x.CurrencyCode).NotEmpty().Matches("^[A-Z]{3}$");
    }
}
