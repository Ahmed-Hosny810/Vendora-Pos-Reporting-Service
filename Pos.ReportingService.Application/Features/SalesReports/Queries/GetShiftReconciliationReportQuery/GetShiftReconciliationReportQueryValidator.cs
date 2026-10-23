using FluentValidation;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;

public class GetShiftReconciliationReportQueryValidator : AbstractValidator<GetShiftReconciliationReportQuery>
{
    public GetShiftReconciliationReportQueryValidator()
    {
        RuleFor(x => x.Parameter).NotNull();
        When(x => x.Parameter != null, () =>
        {
            RuleFor(x => x.Parameter.PageNumber).GreaterThan(0);
            RuleFor(x => x.Parameter.PageSize).InclusiveBetween(1, 50);
            RuleFor(x => x.Parameter.OrderKey).IsInEnum();
            RuleFor(x => x.Parameter.Filter).NotNull().SetValidator(new ShiftReconciliationReportFilterValidator());
        });
    }
}

public class ShiftReconciliationReportFilterValidator : AbstractValidator<ShiftReconciliationReportFilter>
{
    public ShiftReconciliationReportFilterValidator()
    {
        RuleFor(x => x.BranchId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        RuleFor(x => x.TerminalId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        RuleFor(x => x.CashierUserId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        RuleFor(x => x.ToUtcExclusive)
            .Must((filter, to) => !filter.FromUtc.HasValue || !to.HasValue || to.Value > filter.FromUtc.Value)
            .WithMessage("ToUtcExclusive must be after FromUtc when both are supplied.");
        RuleFor(x => x.Status)
            .Must(status => status == null || status == ShiftReportStatuses.Open || status == ShiftReportStatuses.Closed)
            .WithMessage("Status must be Open or Closed when supplied.");
    }
}
