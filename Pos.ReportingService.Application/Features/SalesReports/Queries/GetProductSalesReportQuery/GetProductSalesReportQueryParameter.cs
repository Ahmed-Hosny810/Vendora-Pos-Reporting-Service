using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.SalesService.Application.Parameters;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;

public class GetProductSalesReportQueryParameter : RequestParameter<ProductSalesOrderKey>
{
    public GetProductSalesReportQueryParameter()
    {
        OrderKey = ProductSalesOrderKey.SalesTotal;
        OrderDescending = true;
    }

    public ProductSalesReportFilter Filter { get; set; } = new();
}

public class ProductSalesReportFilter : SalesReportFilter
{
    public Guid? ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
}

public enum ProductSalesOrderKey
{
    QuantitySold = 1,
    SalesTotal = 2,
    SalesTotalAfterReturns = 3
}
