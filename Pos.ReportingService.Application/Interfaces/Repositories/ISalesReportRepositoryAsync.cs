using Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;
using Pos.SalesService.Application.Wrappers;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;

namespace Pos.ReportingService.Application.Interfaces.Repositories;

public interface ISalesReportRepositoryAsync
{
    Task<SalesDashboardDto> GetDashboardAsync(Guid tenantId, SalesReportFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<DailySalesReportDto>> GetDailySalesReportAsync(Guid tenantId, DailySalesReportFilter filter, CancellationToken cancellationToken);
    Task<PagedResponse<IEnumerable<ProductSalesReportDto>>> GetProductSalesReportAsync(
        Guid tenantId, GetProductSalesReportQueryParameter parameter, CancellationToken cancellationToken);
    Task<PagedResponse<IEnumerable<BranchSalesReportDto>>> GetBranchSalesReportAsync(
        Guid tenantId, GetBranchSalesReportQueryParameter parameter, CancellationToken cancellationToken);
    Task<PagedResponse<IEnumerable<CashierSalesReportDto>>> GetCashierSalesReportAsync(
        Guid tenantId, GetCashierSalesReportQueryParameter parameter, CancellationToken cancellationToken);
    Task<PagedResponse<IEnumerable<PaymentMethodReportDto>>> GetPaymentMethodReportAsync(
        Guid tenantId, GetPaymentMethodReportQueryParameter parameter, CancellationToken cancellationToken);
    Task<PagedResponse<IEnumerable<ReturnsRefundsReportDto>>> GetReturnsRefundsReportAsync(
        Guid tenantId, GetReturnsRefundsReportQueryParameter parameter, CancellationToken cancellationToken);
    Task<PagedResponse<IEnumerable<ShiftReconciliationReportDto>>> GetShiftReconciliationReportAsync(
        Guid tenantId, GetShiftReconciliationReportQueryParameter parameter, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductSalesReportDto>> GetProductSalesForExportAsync(
        Guid tenantId, GetProductSalesReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken);
    Task<IReadOnlyList<BranchSalesReportDto>> GetBranchSalesForExportAsync(
        Guid tenantId, GetBranchSalesReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken);
    Task<IReadOnlyList<CashierSalesReportDto>> GetCashierSalesForExportAsync(
        Guid tenantId, GetCashierSalesReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken);
    Task<IReadOnlyList<PaymentMethodReportDto>> GetPaymentMethodForExportAsync(
        Guid tenantId, GetPaymentMethodReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReturnsRefundsReportDto>> GetReturnsRefundsForExportAsync(
        Guid tenantId, GetReturnsRefundsReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken);
    Task<IReadOnlyList<ShiftReconciliationReportDto>> GetShiftReconciliationForExportAsync(
        Guid tenantId, GetShiftReconciliationReportQueryParameter parameter, int maxRows, CancellationToken cancellationToken);
    Task<IReadOnlyList<DailySalesReportDto>> GetDailySalesForExportAsync(
        Guid tenantId, DailySalesReportFilter filter, int maxRows, CancellationToken cancellationToken);
}
