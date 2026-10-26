using Pos.ReportingService.Application.Features.SalesReports.Queries.GetDailySalesQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using FluentValidation;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportDailySalesReportQuery;

public class ExportDailySalesReportQueryValidator : AbstractValidator<ExportDailySalesReportQuery>
{
    public ExportDailySalesReportQueryValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = new GetDailySalesReportQueryValidator().Validate(new Pos.ReportingService.Application.Features.SalesReports.Queries.GetDailySalesQuery.GetDailySalesReportQuery { Filter = request.Filter });
            foreach (var error in result.Errors)
            {
                var propertyName = error.PropertyName.StartsWith("Parameter.")
                    ? error.PropertyName["Parameter.".Length..] : error.PropertyName;
                context.AddFailure(propertyName, error.ErrorMessage);
            }
        });
    }
}
