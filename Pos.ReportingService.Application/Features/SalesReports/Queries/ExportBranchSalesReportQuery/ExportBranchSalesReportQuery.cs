using Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportBranchSalesReportQuery;

public class ExportBranchSalesReportQuery : IRequest<Result<ReportExportResult>>
{
    public SalesReportFilter Filter { get; set; } = new();
    public BranchSalesOrderKey OrderKey { get; set; } = BranchSalesOrderKey.SalesTotal;
    public bool OrderDescending { get; set; } = true;
}

public class ExportBranchSalesReportQueryHandler : IRequestHandler<ExportBranchSalesReportQuery, Result<ReportExportResult>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csv;

    public ExportBranchSalesReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser, ICsvExportService csv)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csv = csv;
    }

    public async Task<Result<ReportExportResult>> Handle(ExportBranchSalesReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");
        var parameter = new GetBranchSalesReportQueryParameter
        { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending };
        var rows = await _repository.GetBranchSalesForExportAsync(tenantId.Value, parameter, _csv.MaxRows, cancellationToken);
        if (rows.Count > _csv.MaxRows)
            return Result<ReportExportResult>.Failure($"The export exceeds {_csv.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Branch Id", "Branch Name", "Currency Code", "Completed Sale Count", "Sales Total", "Discount Total", "Sales Tax Total", "Accepted Return Amount", "Sales Total After Returns" };
        var values = rows.Select(row => (IReadOnlyList<object?>)new object?[] { row.BranchId, row.BranchName, row.CurrencyCode, row.CompletedSaleCount, row.SalesTotal, row.DiscountTotal, row.SalesTaxTotal, row.AcceptedReturnAmount, row.SalesTotalAfterReturns });
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = _csv.Write(headers, values, cancellationToken),
            FileName = $"branch-sales-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
