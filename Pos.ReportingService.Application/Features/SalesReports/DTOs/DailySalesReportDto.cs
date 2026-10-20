namespace Pos.ReportingService.Application.Features.SalesReports.DTOs;

/// <summary>Sales and accepted returns for one day in the report's business timezone.</summary>
public class DailySalesReportDto
{
    public DateOnly Date { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int CompletedSaleCount { get; set; }
    /// <summary>After discounts, including tax, before returns.</summary>
    public decimal SalesTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    /// <summary>Tax on sales completed that day, before return tax adjustments.</summary>
    public decimal SalesTaxTotal { get; set; }
    /// <summary>Returns accepted that day, regardless of the original sale date or refund payment date.</summary>
    public decimal AcceptedReturnAmount { get; set; }
    /// <summary>SalesTotal minus AcceptedReturnAmount; can be negative.</summary>
    public decimal SalesTotalAfterReturns { get; set; }
}
