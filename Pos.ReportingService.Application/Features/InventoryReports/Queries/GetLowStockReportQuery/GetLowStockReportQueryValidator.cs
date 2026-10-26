using FluentValidation;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;

public class GetLowStockReportQueryValidator : AbstractValidator<GetLowStockReportQuery>
{
    public GetLowStockReportQueryValidator()
    {
        RuleFor(x => x.Parameter).NotNull();
        When(x => x.Parameter != null, () =>
        {
            RuleFor(x => x.Parameter.PageNumber).GreaterThan(0);
            RuleFor(x => x.Parameter.PageSize).InclusiveBetween(1, 50);
            RuleFor(x => x.Parameter.OrderKey).IsInEnum();
            RuleFor(x => x.Parameter.Filter).NotNull().SetValidator(new LowStockReportFilterValidator());
        });
    }
}

public class LowStockReportFilterValidator : AbstractValidator<LowStockReportFilter>
{
    public LowStockReportFilterValidator()
    {
        RuleFor(x => x.BranchId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        RuleFor(x => x.ProductId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        RuleFor(x => x.ProductVariantId).Must(id => !id.HasValue || id.Value != Guid.Empty);
    }
}
