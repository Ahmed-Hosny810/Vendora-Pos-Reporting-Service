using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery;

namespace Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

public static class BranchSalesReportQueryExtensions
{
    public static IOrderedQueryable<BranchSalesReportDto> ApplyOrder(this IQueryable<BranchSalesReportDto> query,
        GetBranchSalesReportQueryParameter parameter)
    {
        var ordered = parameter.OrderKey switch
        {
            BranchSalesOrderKey.CompletedSaleCount => parameter.OrderDescending
                ? query.OrderByDescending(x => x.CompletedSaleCount) : query.OrderBy(x => x.CompletedSaleCount),
            BranchSalesOrderKey.SalesTotal => parameter.OrderDescending
                ? query.OrderByDescending(x => x.SalesTotal) : query.OrderBy(x => x.SalesTotal),
            BranchSalesOrderKey.SalesTotalAfterReturns => parameter.OrderDescending
                ? query.OrderByDescending(x => x.SalesTotalAfterReturns) : query.OrderBy(x => x.SalesTotalAfterReturns),
            BranchSalesOrderKey.AcceptedReturnAmount => parameter.OrderDescending
                ? query.OrderByDescending(x => x.AcceptedReturnAmount) : query.OrderBy(x => x.AcceptedReturnAmount),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter.OrderKey))
        };
        return ordered.ThenBy(x => x.BranchId);
    }
}
