using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.SalesService.Application.Parameters;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;

public class GetCashierSalesReportQueryParameter : RequestParameter<CashierSalesOrderKey>
{
    public GetCashierSalesReportQueryParameter()
    {
        OrderKey = CashierSalesOrderKey.SalesTotal;
        OrderDescending = true;
    }
    public CashierSalesReportFilter Filter { get; set; } = new();
}

public class CashierSalesReportFilter : SalesReportFilter
{
    public Guid? CashierUserId { get; set; }
}

public enum CashierSalesOrderKey
{
    CompletedSaleCount,
    SalesTotal,
    DiscountTotal,
    SalesTaxTotal
}
