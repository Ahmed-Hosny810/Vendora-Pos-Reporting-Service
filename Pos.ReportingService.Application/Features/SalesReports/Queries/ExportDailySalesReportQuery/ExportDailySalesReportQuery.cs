using Pos.ReportingService.Application.Features.SalesReports.Queries.GetDailySalesQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportDailySalesReportQuery;

public class ExportDailySalesReportQuery : IRequest<Result<ReportExportResult>>
{
    public DailySalesReportFilter Filter { get; set; } = new();
}

public class ExportDailySalesReportQueryHandler : IRequestHandler<ExportDailySalesReportQuery, Result<ReportExportResult>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csv;

    public ExportDailySalesReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser, ICsvExportService csv)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csv = csv;
    }

    public async Task<Result<ReportExportResult>> Handle(ExportDailySalesReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");
        var rows = await _repository.GetDailySalesForExportAsync(tenantId.Value, request.Filter, _csv.MaxRows, cancellationToken);
        if (rows.Count > _csv.MaxRows)
            return Result<ReportExportResult>.Failure($"The export exceeds {_csv.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Date", "Currency Code", "Completed Sale Count", "Sales Total", "Discount Total", "Sales Tax Total", "Accepted Return Amount", "Sales Total After Returns" };
        var values = rows.Select(row => (IReadOnlyList<object?>)new object?[] { row.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), row.CurrencyCode, row.CompletedSaleCount, row.SalesTotal, row.DiscountTotal, row.SalesTaxTotal, row.AcceptedReturnAmount, row.SalesTotalAfterReturns });
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = _csv.Write(headers, values, cancellationToken),
            FileName = $"daily-sales-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
