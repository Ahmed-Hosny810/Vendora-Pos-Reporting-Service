using Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using FluentValidation;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportReturnsRefundsReportQuery;

public class ExportReturnsRefundsReportQueryValidator : AbstractValidator<ExportReturnsRefundsReportQuery>
{
    public ExportReturnsRefundsReportQueryValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = new GetReturnsRefundsReportQueryValidator().Validate(new Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery.GetReturnsRefundsReportQuery { Parameter = new GetReturnsRefundsReportQueryParameter { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending } });
            foreach (var error in result.Errors)
            {
                var propertyName = error.PropertyName.StartsWith("Parameter.")
                    ? error.PropertyName["Parameter.".Length..] : error.PropertyName;
                context.AddFailure(propertyName, error.ErrorMessage);
            }
        });
    }
}
