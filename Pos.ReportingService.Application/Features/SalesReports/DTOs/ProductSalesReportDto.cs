namespace Pos.ReportingService.Application.Features.SalesReports.DTOs;

public class ProductSalesReportDto
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal QuantityReturned { get; set; }
    public decimal NetQuantitySold { get; set; }
    /// <summary>Saved line totals after discounts, including tax, before returns.</summary>
    public decimal SalesTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal SalesTaxTotal { get; set; }
    /// <summary>Accepted return value in the period, independent of refund payment.</summary>
    public decimal AcceptedReturnAmount { get; set; }
    public decimal SalesTotalAfterReturns { get; set; }
}
