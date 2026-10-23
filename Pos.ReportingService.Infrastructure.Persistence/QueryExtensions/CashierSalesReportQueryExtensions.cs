using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.QueryExtensions
{
    public static class CashierSalesReportQueryExtensions
    {
        public static IQueryable<CashierShiftReadModel> ApplyCashierFilter(
            this IQueryable<CashierShiftReadModel> query,
            Guid tenantId,
            CashierSalesReportFilter filter)
        {
            return query.Where(shift =>
                shift.TenantId == tenantId &&
                (!filter.CashierUserId.HasValue ||
                 shift.CashierUserId == filter.CashierUserId.Value));
        }

        public static IOrderedQueryable<CashierSalesReportDto> ApplyOrder(
            this IQueryable<CashierSalesReportDto> query,
            GetCashierSalesReportQueryParameter parameter)
        {
            var ordered = parameter.OrderKey switch
            {
                CashierSalesOrderKey.CompletedSaleCount => parameter.OrderDescending
                    ? query.OrderByDescending(x => x.CompletedSaleCount)
                    : query.OrderBy(x => x.CompletedSaleCount),

                CashierSalesOrderKey.SalesTotal => parameter.OrderDescending
                    ? query.OrderByDescending(x => x.SalesTotal)
                    : query.OrderBy(x => x.SalesTotal),

                CashierSalesOrderKey.DiscountTotal => parameter.OrderDescending
                    ? query.OrderByDescending(x => x.DiscountTotal)
                    : query.OrderBy(x => x.DiscountTotal),

                CashierSalesOrderKey.SalesTaxTotal => parameter.OrderDescending
                    ? query.OrderByDescending(x => x.SalesTaxTotal)
                    : query.OrderBy(x => x.SalesTaxTotal),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(parameter.OrderKey))
            };

            return ordered.ThenBy(x => x.CashierUserId);
        }
    }
}
