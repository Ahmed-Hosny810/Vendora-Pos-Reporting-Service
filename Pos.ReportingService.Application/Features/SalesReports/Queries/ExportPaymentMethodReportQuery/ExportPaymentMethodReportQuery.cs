using Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportPaymentMethodReportQuery;

public class ExportPaymentMethodReportQuery : IRequest<Result<ReportExportResult>>
{
    public SalesReportFilter Filter { get; set; } = new();
    public PaymentMethodOrderKey OrderKey { get; set; } = PaymentMethodOrderKey.CollectedAmount;
    public bool OrderDescending { get; set; } = true;
}

public class ExportPaymentMethodReportQueryHandler : IRequestHandler<ExportPaymentMethodReportQuery, Result<ReportExportResult>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csv;

    public ExportPaymentMethodReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser, ICsvExportService csv)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csv = csv;
    }

    public async Task<Result<ReportExportResult>> Handle(ExportPaymentMethodReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");
        var parameter = new GetPaymentMethodReportQueryParameter
        { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending };
        var rows = await _repository.GetPaymentMethodForExportAsync(tenantId.Value, parameter, _csv.MaxRows, cancellationToken);
        if (rows.Count > _csv.MaxRows)
            return Result<ReportExportResult>.Failure($"The export exceeds {_csv.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Payment Method Id", "Payment Method Name", "Currency Code", "Completed Payment Count", "Collected Amount", "Refund Amount", "Net Collected Amount" };
        var values = rows.Select(row => (IReadOnlyList<object?>)new object?[] { row.PaymentMethodId, row.PaymentMethodName, row.CurrencyCode, row.CompletedPaymentCount, row.CollectedAmount, row.RefundAmount, row.NetCollectedAmount });
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = _csv.Write(headers, values, cancellationToken),
            FileName = $"payment-methods-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
