using Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportCashierSalesReportQuery;

public class ExportCashierSalesReportQuery : IRequest<Result<ReportExportResult>>
{
    public CashierSalesReportFilter Filter { get; set; } = new();
    public CashierSalesOrderKey OrderKey { get; set; } = CashierSalesOrderKey.SalesTotal;
    public bool OrderDescending { get; set; } = true;
}

public class ExportCashierSalesReportQueryHandler : IRequestHandler<ExportCashierSalesReportQuery, Result<ReportExportResult>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csv;

    public ExportCashierSalesReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser, ICsvExportService csv)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csv = csv;
    }

    public async Task<Result<ReportExportResult>> Handle(ExportCashierSalesReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");
        var parameter = new GetCashierSalesReportQueryParameter
        { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending };
        var rows = await _repository.GetCashierSalesForExportAsync(tenantId.Value, parameter, _csv.MaxRows, cancellationToken);
        if (rows.Count > _csv.MaxRows)
            return Result<ReportExportResult>.Failure($"The export exceeds {_csv.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Cashier User Id", "Cashier Name", "Currency Code", "Completed Sale Count", "Sales Total", "Discount Total", "Sales Tax Total" };
        var values = rows.Select(row => (IReadOnlyList<object?>)new object?[] { row.CashierUserId, row.CashierName, row.CurrencyCode, row.CompletedSaleCount, row.SalesTotal, row.DiscountTotal, row.SalesTaxTotal });
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = _csv.Write(headers, values, cancellationToken),
            FileName = $"cashier-sales-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
