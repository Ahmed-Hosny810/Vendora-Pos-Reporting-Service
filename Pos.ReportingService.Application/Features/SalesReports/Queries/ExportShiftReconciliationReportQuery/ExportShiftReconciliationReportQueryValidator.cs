using Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using FluentValidation;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportShiftReconciliationReportQuery;

public class ExportShiftReconciliationReportQueryValidator : AbstractValidator<ExportShiftReconciliationReportQuery>
{
    public ExportShiftReconciliationReportQueryValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = new GetShiftReconciliationReportQueryValidator().Validate(new Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery.GetShiftReconciliationReportQuery { Parameter = new GetShiftReconciliationReportQueryParameter { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending } });
            foreach (var error in result.Errors)
            {
                var propertyName = error.PropertyName.StartsWith("Parameter.")
                    ? error.PropertyName["Parameter.".Length..] : error.PropertyName;
                context.AddFailure(propertyName, error.ErrorMessage);
            }
        });
    }
}
