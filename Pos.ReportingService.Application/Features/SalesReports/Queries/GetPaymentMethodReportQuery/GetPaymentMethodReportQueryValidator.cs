using FluentValidation;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;

public class GetPaymentMethodReportQueryValidator : AbstractValidator<GetPaymentMethodReportQuery>
{
    public GetPaymentMethodReportQueryValidator()
    {
        RuleFor(x => x.Parameter).NotNull();
        When(x => x.Parameter != null, () =>
        {
            RuleFor(x => x.Parameter.PageNumber).GreaterThan(0);
            RuleFor(x => x.Parameter.PageSize).InclusiveBetween(1, 50);
            RuleFor(x => x.Parameter.OrderKey).IsInEnum();
            RuleFor(x => x.Parameter.Filter).NotNull().SetValidator(new SalesReportFilterValidator());
        });
    }
}
