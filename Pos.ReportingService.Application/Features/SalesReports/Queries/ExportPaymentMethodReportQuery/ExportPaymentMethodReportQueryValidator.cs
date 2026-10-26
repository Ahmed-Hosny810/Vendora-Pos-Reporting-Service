using Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using FluentValidation;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportPaymentMethodReportQuery;

public class ExportPaymentMethodReportQueryValidator : AbstractValidator<ExportPaymentMethodReportQuery>
{
    public ExportPaymentMethodReportQueryValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = new GetPaymentMethodReportQueryValidator().Validate(new Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery.GetPaymentMethodReportQuery { Parameter = new GetPaymentMethodReportQueryParameter { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending } });
            foreach (var error in result.Errors)
            {
                var propertyName = error.PropertyName.StartsWith("Parameter.")
                    ? error.PropertyName["Parameter.".Length..] : error.PropertyName;
                context.AddFailure(propertyName, error.ErrorMessage);
            }
        });
    }
}
