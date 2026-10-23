namespace Pos.ReportingService.Application.Features.SalesReports.DTOs;

/// <summary>Issued sale totals attributed to the shift cashier, before deducting returns.</summary>
public class CashierSalesReportDto
{
    public Guid CashierUserId { get; set; }
    /// <summary>Optional display name when an authorized Identity lookup is available.</summary>
    public string? CashierName { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int CompletedSaleCount { get; set; }
    /// <summary>After discounts, including tax.</summary>
    public decimal SalesTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal SalesTaxTotal { get; set; }
}
