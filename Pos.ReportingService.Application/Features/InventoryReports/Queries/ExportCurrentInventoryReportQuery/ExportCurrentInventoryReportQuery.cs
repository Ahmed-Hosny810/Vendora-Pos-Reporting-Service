using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.ExportCurrentInventoryReportQuery;

public class ExportCurrentInventoryReportQuery : IRequest<Result<ReportExportResult>>
{
    public CurrentInventoryReportFilter Filter { get; set; } = new();
    public CurrentInventoryOrderKey OrderKey { get; set; } = CurrentInventoryOrderKey.QuantityAvailable;
    public bool OrderDescending { get; set; }
}

public class ExportCurrentInventoryReportQueryHandler
    : IRequestHandler<ExportCurrentInventoryReportQuery, Result<ReportExportResult>>
{
    private readonly IInventoryReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csvExportService;

    public ExportCurrentInventoryReportQueryHandler(IInventoryReportRepositoryAsync repository,
        ICurrentUserService currentUser, ICsvExportService csvExportService)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csvExportService = csvExportService;
    }

    public async Task<Result<ReportExportResult>> Handle(
        ExportCurrentInventoryReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");

        var rows = await _repository.GetCurrentInventoryForExportAsync(tenantId.Value,
            request.Filter, request.OrderKey, request.OrderDescending,
            _csvExportService.MaxRows, cancellationToken);

        if (rows.Count > _csvExportService.MaxRows)
            return Result<ReportExportResult>.Failure(
                $"The export exceeds {_csvExportService.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Branch", "Product", "Variant", "On Hand", "Reserved", "Available", "Low-stock Threshold" };
        var csvRows = rows.Select(row => (IReadOnlyList<object?>)new object?[]
        {
            row.BranchName, row.ProductName, row.VariantName, row.QuantityOnHand,
            row.QuantityReserved, row.QuantityAvailable, row.LowStockThreshold
        });
        var content = _csvExportService.Write(headers, csvRows, cancellationToken);
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = content,
            FileName = $"current-inventory-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
