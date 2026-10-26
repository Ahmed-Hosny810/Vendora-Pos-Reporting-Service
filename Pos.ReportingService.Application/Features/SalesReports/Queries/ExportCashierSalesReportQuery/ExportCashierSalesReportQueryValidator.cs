using Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using FluentValidation;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportCashierSalesReportQuery;

public class ExportCashierSalesReportQueryValidator : AbstractValidator<ExportCashierSalesReportQuery>
{
    public ExportCashierSalesReportQueryValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = new GetCashierSalesReportQueryValidator().Validate(new Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery.GetCashierSalesReportQuery { Parameter = new GetCashierSalesReportQueryParameter { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending } });
            foreach (var error in result.Errors)
            {
                var propertyName = error.PropertyName.StartsWith("Parameter.")
                    ? error.PropertyName["Parameter.".Length..] : error.PropertyName;
                context.AddFailure(propertyName, error.ErrorMessage);
            }
        });
    }
}
