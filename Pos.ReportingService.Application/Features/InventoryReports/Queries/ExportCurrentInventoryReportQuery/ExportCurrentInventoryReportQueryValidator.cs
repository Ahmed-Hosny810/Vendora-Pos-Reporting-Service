using FluentValidation;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.ExportCurrentInventoryReportQuery;

public class ExportCurrentInventoryReportQueryValidator : AbstractValidator<ExportCurrentInventoryReportQuery>
{
    public ExportCurrentInventoryReportQueryValidator()
    {
        RuleFor(x => x.Filter).NotNull().SetValidator(new CurrentInventoryReportFilterValidator());
        RuleFor(x => x.OrderKey).IsInEnum();
    }
}
