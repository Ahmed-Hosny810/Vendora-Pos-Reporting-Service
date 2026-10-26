using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using FluentValidation;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.ExportLowStockReportQuery;

public class ExportLowStockReportQueryValidator : AbstractValidator<ExportLowStockReportQuery>
{
    public ExportLowStockReportQueryValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = new GetLowStockReportQueryValidator().Validate(new Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery.GetLowStockReportQuery { Parameter = new GetLowStockReportQueryParameter { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending } });
            foreach (var error in result.Errors)
            {
                var propertyName = error.PropertyName.StartsWith("Parameter.")
                    ? error.PropertyName["Parameter.".Length..] : error.PropertyName;
                context.AddFailure(propertyName, error.ErrorMessage);
            }
        });
    }
}
