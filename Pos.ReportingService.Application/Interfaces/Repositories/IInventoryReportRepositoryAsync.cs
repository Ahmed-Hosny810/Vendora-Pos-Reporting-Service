using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;
using Pos.ReportingService.Application.Features.InventoryReports.DTOs;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Interfaces.Repositories;

public interface IInventoryReportRepositoryAsync
{
    Task<PagedResponse<IEnumerable<CurrentInventoryReportDto>>> GetCurrentInventoryReportAsync(
        Guid tenantId, GetCurrentInventoryReportQueryParameter parameter, CancellationToken cancellationToken);
    Task<PagedResponse<IEnumerable<LowStockReportDto>>> GetLowStockReportAsync(
        Guid tenantId, GetLowStockReportQueryParameter parameter, CancellationToken cancellationToken);
    Task<IReadOnlyList<CurrentInventoryReportDto>> GetCurrentInventoryForExportAsync(
        Guid tenantId, CurrentInventoryReportFilter filter, CurrentInventoryOrderKey orderKey,
        bool orderDescending, int maxRows, CancellationToken cancellationToken);
    Task<IReadOnlyList<LowStockReportDto>> GetLowStockForExportAsync(
        Guid tenantId, GetLowStockReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken);
}
