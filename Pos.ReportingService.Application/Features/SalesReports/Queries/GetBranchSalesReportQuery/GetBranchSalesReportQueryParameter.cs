using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.SalesService.Application.Parameters;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery;

public class GetBranchSalesReportQueryParameter : RequestParameter<BranchSalesOrderKey>
{
    public GetBranchSalesReportQueryParameter()
    {
        OrderKey = BranchSalesOrderKey.SalesTotal;
        OrderDescending = true;
    }
    public SalesReportFilter Filter { get; set; } = new();
}

public enum BranchSalesOrderKey
{
    CompletedSaleCount,
    SalesTotal,
    SalesTotalAfterReturns,
    AcceptedReturnAmount
}
