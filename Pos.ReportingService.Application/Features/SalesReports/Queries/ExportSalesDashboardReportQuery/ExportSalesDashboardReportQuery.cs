using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportSalesDashboardReportQuery;

public class ExportSalesDashboardReportQuery : IRequest<Result<ReportExportResult>>
{
    public SalesReportFilter Filter { get; set; } = new();
}

public class ExportSalesDashboardReportQueryHandler : IRequestHandler<ExportSalesDashboardReportQuery, Result<ReportExportResult>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csv;

    public ExportSalesDashboardReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser, ICsvExportService csv)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csv = csv;
    }

    public async Task<Result<ReportExportResult>> Handle(ExportSalesDashboardReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");
        var rows = new[] { await _repository.GetDashboardAsync(tenantId.Value, request.Filter, cancellationToken) };
        if (rows.Length > _csv.MaxRows)
            return Result<ReportExportResult>.Failure($"The export exceeds {_csv.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Currency Code", "Completed Sale Count", "Sales Total", "Discount Total", "Sales Tax Total", "Accepted Return Amount", "Sales Total After Returns" };
        var values = rows.Select(row => (IReadOnlyList<object?>)new object?[] { row.CurrencyCode, row.CompletedSaleCount, row.SalesTotal, row.DiscountTotal, row.SalesTaxTotal, row.AcceptedReturnAmount, row.SalesTotalAfterReturns });
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = _csv.Write(headers, values, cancellationToken),
            FileName = $"sales-dashboard-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
