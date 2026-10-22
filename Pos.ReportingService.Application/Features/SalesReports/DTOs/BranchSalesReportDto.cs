namespace Pos.ReportingService.Application.Features.SalesReports.DTOs;

public class BranchSalesReportDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public int CompletedSaleCount { get; set; }
    /// <summary>After discounts, including tax, before returns.</summary>
    public decimal SalesTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal SalesTaxTotal { get; set; }
    /// <summary>Returns accepted at this branch in the period, whether refunded yet or not.</summary>
    public decimal AcceptedReturnAmount { get; set; }
    public decimal SalesTotalAfterReturns { get; set; }
}
