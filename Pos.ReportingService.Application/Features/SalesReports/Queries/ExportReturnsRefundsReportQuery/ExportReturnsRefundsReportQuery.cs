using Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportReturnsRefundsReportQuery;

public class ExportReturnsRefundsReportQuery : IRequest<Result<ReportExportResult>>
{
    public ReturnsRefundsReportFilter Filter { get; set; } = new();
    public ReturnsRefundsOrderKey OrderKey { get; set; } = ReturnsRefundsOrderKey.CompletedAt;
    public bool OrderDescending { get; set; } = true;
}

public class ExportReturnsRefundsReportQueryHandler : IRequestHandler<ExportReturnsRefundsReportQuery, Result<ReportExportResult>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csv;

    public ExportReturnsRefundsReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser, ICsvExportService csv)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csv = csv;
    }

    public async Task<Result<ReportExportResult>> Handle(ExportReturnsRefundsReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");
        var parameter = new GetReturnsRefundsReportQueryParameter
        { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending };
        var rows = await _repository.GetReturnsRefundsForExportAsync(tenantId.Value, parameter, _csv.MaxRows, cancellationToken);
        if (rows.Count > _csv.MaxRows)
            return Result<ReportExportResult>.Failure($"The export exceeds {_csv.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Return Id", "Return Number", "Original Sale Id", "Original Receipt Number", "Completed At", "Branch Id", "Branch Name", "Processed By User Id", "Currency Code", "Authorized Refund Amount", "Refunded Amount", "Remaining Refund Amount", "Refund State" };
        var values = rows.Select(row => (IReadOnlyList<object?>)new object?[] { row.ReturnId, row.ReturnNumber, row.OriginalSaleId, row.OriginalReceiptNumber, row.CompletedAt, row.BranchId, row.BranchName, row.ProcessedByUserId, row.CurrencyCode, row.AuthorizedRefundAmount, row.RefundedAmount, row.RemainingRefundAmount, row.RefundState });
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = _csv.Write(headers, values, cancellationToken),
            FileName = $"returns-refunds-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
