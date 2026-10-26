using Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using FluentValidation;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportProductSalesReportQuery;

public class ExportProductSalesReportQueryValidator : AbstractValidator<ExportProductSalesReportQuery>
{
    public ExportProductSalesReportQueryValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = new GetProductSalesReportQueryValidator().Validate(new Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery.GetProductSalesReportQuery { Parameter = new GetProductSalesReportQueryParameter { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending } });
            foreach (var error in result.Errors)
            {
                var propertyName = error.PropertyName.StartsWith("Parameter.")
                    ? error.PropertyName["Parameter.".Length..] : error.PropertyName;
                context.AddFailure(propertyName, error.ErrorMessage);
            }
        });
    }
}
