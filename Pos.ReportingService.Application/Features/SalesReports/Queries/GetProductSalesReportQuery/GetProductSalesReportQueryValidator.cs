using FluentValidation;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;

public class GetProductSalesReportQueryValidator : AbstractValidator<GetProductSalesReportQuery>
{
    public GetProductSalesReportQueryValidator()
    {
        RuleFor(x => x.Parameter).NotNull();
        When(x => x.Parameter != null, () =>
        {
            RuleFor(x => x.Parameter.PageNumber).GreaterThan(0);
            RuleFor(x => x.Parameter.PageSize).InclusiveBetween(1, 50);
            RuleFor(x => x.Parameter.OrderKey).IsInEnum();
            RuleFor(x => x.Parameter.Filter).NotNull()
                .SetValidator(new ProductSalesReportFilterValidator());
        });
    }
}

public class ProductSalesReportFilterValidator : AbstractValidator<ProductSalesReportFilter>
{
    public ProductSalesReportFilterValidator()
    {
        Include(new SalesReportFilterValidator());
        RuleFor(x => x.ProductId).Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("ProductId must be a valid non-empty ID when supplied.");
        RuleFor(x => x.ProductVariantId).Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("ProductVariantId must be a valid non-empty ID when supplied.");
    }
}
