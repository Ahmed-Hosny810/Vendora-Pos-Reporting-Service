using Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportShiftReconciliationReportQuery;

public class ExportShiftReconciliationReportQuery : IRequest<Result<ReportExportResult>>
{
    public ShiftReconciliationReportFilter Filter { get; set; } = new();
    public ShiftReconciliationOrderKey OrderKey { get; set; } = ShiftReconciliationOrderKey.OpenedAt;
    public bool OrderDescending { get; set; } = true;
}

public class ExportShiftReconciliationReportQueryHandler : IRequestHandler<ExportShiftReconciliationReportQuery, Result<ReportExportResult>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csv;

    public ExportShiftReconciliationReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser, ICsvExportService csv)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csv = csv;
    }

    public async Task<Result<ReportExportResult>> Handle(ExportShiftReconciliationReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");
        var parameter = new GetShiftReconciliationReportQueryParameter
        { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending };
        var rows = await _repository.GetShiftReconciliationForExportAsync(tenantId.Value, parameter, _csv.MaxRows, cancellationToken);
        if (rows.Count > _csv.MaxRows)
            return Result<ReportExportResult>.Failure($"The export exceeds {_csv.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Shift Id", "Branch Id", "Branch Name", "Terminal Id", "Cashier User Id", "Opened At", "Closed At", "Status", "Opening Cash", "Cash Collected", "Cash Refunded", "Expected Cash", "Closing Cash", "Difference" };
        var values = rows.Select(row => (IReadOnlyList<object?>)new object?[] { row.ShiftId, row.BranchId, row.BranchName, row.TerminalId, row.CashierUserId, row.OpenedAt, row.ClosedAt, row.Status, row.OpeningCash, row.CashCollected, row.CashRefunded, row.ExpectedCash, row.ClosingCash, row.Difference });
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = _csv.Write(headers, values, cancellationToken),
            FileName = $"shift-reconciliation-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
