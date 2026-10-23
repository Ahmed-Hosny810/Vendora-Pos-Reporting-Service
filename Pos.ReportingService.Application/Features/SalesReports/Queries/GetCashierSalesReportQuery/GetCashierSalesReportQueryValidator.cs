using FluentValidation;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;

public class GetCashierSalesReportQueryValidator : AbstractValidator<GetCashierSalesReportQuery>
{
    public GetCashierSalesReportQueryValidator()
    {
        RuleFor(x => x.Parameter).NotNull();
        When(x => x.Parameter != null, () =>
        {
            RuleFor(x => x.Parameter.PageNumber).GreaterThan(0);
            RuleFor(x => x.Parameter.PageSize).InclusiveBetween(1, 50);
            RuleFor(x => x.Parameter.OrderKey).IsInEnum();
            RuleFor(x => x.Parameter.Filter).NotNull().SetValidator(new CashierSalesReportFilterValidator());
        });
    }
}

public class CashierSalesReportFilterValidator : AbstractValidator<CashierSalesReportFilter>
{
    public CashierSalesReportFilterValidator()
    {
        Include(new SalesReportFilterValidator());
        RuleFor(x => x.CashierUserId).Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("CashierUserId must be a valid non-empty ID when supplied.");
    }
}
