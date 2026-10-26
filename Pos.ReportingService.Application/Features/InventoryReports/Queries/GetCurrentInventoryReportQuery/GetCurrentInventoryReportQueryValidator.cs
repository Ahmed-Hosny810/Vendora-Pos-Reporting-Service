using FluentValidation;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;

public class GetCurrentInventoryReportQueryValidator : AbstractValidator<GetCurrentInventoryReportQuery>
{
    public GetCurrentInventoryReportQueryValidator()
    {
        RuleFor(x => x.Parameter).NotNull();
        When(x => x.Parameter != null, () =>
        {
            RuleFor(x => x.Parameter.PageNumber).GreaterThan(0);
            RuleFor(x => x.Parameter.PageSize).InclusiveBetween(1, 50);
            RuleFor(x => x.Parameter.OrderKey).IsInEnum();
            RuleFor(x => x.Parameter.Filter).NotNull().SetValidator(new CurrentInventoryReportFilterValidator());
        });
    }
}

public class CurrentInventoryReportFilterValidator : AbstractValidator<CurrentInventoryReportFilter>
{
    public CurrentInventoryReportFilterValidator()
    {
        RuleFor(x => x.BranchId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        RuleFor(x => x.ProductId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        RuleFor(x => x.ProductVariantId).Must(id => !id.HasValue || id.Value != Guid.Empty);
    }
}
