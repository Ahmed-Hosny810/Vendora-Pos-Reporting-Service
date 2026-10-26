using Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using FluentValidation;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportBranchSalesReportQuery;

public class ExportBranchSalesReportQueryValidator : AbstractValidator<ExportBranchSalesReportQuery>
{
    public ExportBranchSalesReportQueryValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = new GetBranchSalesReportQueryValidator().Validate(new Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery.GetBranchSalesReportQuery { Parameter = new GetBranchSalesReportQueryParameter { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending } });
            foreach (var error in result.Errors)
            {
                var propertyName = error.PropertyName.StartsWith("Parameter.")
                    ? error.PropertyName["Parameter.".Length..] : error.PropertyName;
                context.AddFailure(propertyName, error.ErrorMessage);
            }
        });
    }
}
