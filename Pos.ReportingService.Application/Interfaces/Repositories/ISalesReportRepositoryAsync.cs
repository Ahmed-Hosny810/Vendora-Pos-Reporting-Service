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

    Task<IReadOnlyList<DailySalesReportDto>> GetDailySalesReportAsync(Guid tenantId, SalesReportFilter filter, CancellationToken cancellationToken);
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
}
