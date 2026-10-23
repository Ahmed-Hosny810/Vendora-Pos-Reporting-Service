using FluentValidation;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;

public class GetReturnsRefundsReportQueryValidator : AbstractValidator<GetReturnsRefundsReportQuery>
{
    public GetReturnsRefundsReportQueryValidator()
    {
        RuleFor(x => x.Parameter).NotNull();
        When(x => x.Parameter != null, () =>
        {
            RuleFor(x => x.Parameter.PageNumber).GreaterThan(0);
            RuleFor(x => x.Parameter.PageSize).InclusiveBetween(1, 50);
            RuleFor(x => x.Parameter.OrderKey).IsInEnum();
            RuleFor(x => x.Parameter.Filter).NotNull().SetValidator(new SalesReportFilterValidator());
            When(x => x.Parameter.Filter != null, () =>
            {
                RuleFor(x => x.Parameter.Filter.RefundState)
                    .Must(value => value == null || value == RefundStates.Unpaid ||
                        value == RefundStates.PartiallyRefunded || value == RefundStates.FullyRefunded)
                    .WithMessage("Refund state must be Unpaid, PartiallyRefunded or FullyRefunded.");
            });
        });
    }
}
