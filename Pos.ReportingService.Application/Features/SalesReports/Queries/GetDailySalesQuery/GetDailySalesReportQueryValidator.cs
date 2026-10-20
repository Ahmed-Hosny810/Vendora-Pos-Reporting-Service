using FluentValidation;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetDailySalesQuery;

public class GetDailySalesReportQueryValidator : AbstractValidator<GetDailySalesReportQuery>
{
    public GetDailySalesReportQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull()
            .SetValidator(new SalesReportFilterValidator());
    }
}
