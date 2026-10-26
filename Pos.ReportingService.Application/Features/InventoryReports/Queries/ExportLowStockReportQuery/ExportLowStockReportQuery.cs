using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.ExportLowStockReportQuery;

public class ExportLowStockReportQuery : IRequest<Result<ReportExportResult>>
{
    public LowStockReportFilter Filter { get; set; } = new();
    public LowStockOrderKey OrderKey { get; set; } = LowStockOrderKey.QuantityAvailable;
    public bool OrderDescending { get; set; } = false;
}

public class ExportLowStockReportQueryHandler : IRequestHandler<ExportLowStockReportQuery, Result<ReportExportResult>>
{
    private readonly IInventoryReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csv;

    public ExportLowStockReportQueryHandler(IInventoryReportRepositoryAsync repository, ICurrentUserService currentUser, ICsvExportService csv)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csv = csv;
    }

    public async Task<Result<ReportExportResult>> Handle(ExportLowStockReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");
        var parameter = new GetLowStockReportQueryParameter
        { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending };
        var rows = await _repository.GetLowStockForExportAsync(tenantId.Value, parameter, _csv.MaxRows, cancellationToken);
        if (rows.Count > _csv.MaxRows)
            return Result<ReportExportResult>.Failure($"The export exceeds {_csv.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Branch Id", "Branch Name", "Product Id", "Product Name", "Product Variant Id", "Variant Name", "Quantity On Hand", "Quantity Reserved", "Quantity Available", "Low Stock Threshold", "Active Alert Id", "Detected At" };
        var values = rows.Select(row => (IReadOnlyList<object?>)new object?[] { row.BranchId, row.BranchName, row.ProductId, row.ProductName, row.ProductVariantId, row.VariantName, row.QuantityOnHand, row.QuantityReserved, row.QuantityAvailable, row.LowStockThreshold, row.ActiveAlertId, row.DetectedAt });
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = _csv.Write(headers, values, cancellationToken),
            FileName = $"low-stock-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
