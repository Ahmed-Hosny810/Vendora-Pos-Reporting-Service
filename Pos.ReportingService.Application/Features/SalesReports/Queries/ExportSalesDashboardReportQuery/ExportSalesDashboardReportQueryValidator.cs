using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using FluentValidation;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportSalesDashboardReportQuery;

public class ExportSalesDashboardReportQueryValidator : AbstractValidator<ExportSalesDashboardReportQuery>
{
    public ExportSalesDashboardReportQueryValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = new GetSalesDashboardQueryValidator().Validate(new Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery.GetSalesDashboardQuery { Filter = request.Filter });
            foreach (var error in result.Errors)
            {
                var propertyName = error.PropertyName.StartsWith("Parameter.")
                    ? error.PropertyName["Parameter.".Length..] : error.PropertyName;
                context.AddFailure(propertyName, error.ErrorMessage);
            }
        });
    }
}
