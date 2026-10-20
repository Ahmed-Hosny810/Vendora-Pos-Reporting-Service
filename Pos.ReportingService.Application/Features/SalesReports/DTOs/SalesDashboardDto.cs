namespace Pos.ReportingService.Application.Features.SalesReports.DTOs;

public class SalesDashboardDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public int CompletedSaleCount { get; set; }
    /// <summary>After discounts, including tax, before returns.</summary>
    public decimal SalesTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    /// <summary>Tax on sales in the period, before return tax adjustments.</summary>
    public decimal SalesTaxTotal { get; set; }
    /// <summary>Accepted return value, whether refunded yet or not.</summary>
    public decimal AcceptedReturnAmount { get; set; }
    public decimal SalesTotalAfterReturns { get; set; }
}
