using Pos.SalesService.Application.Parameters;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;

public class GetLowStockReportQueryParameter : RequestParameter<LowStockOrderKey>
{
    public LowStockReportFilter Filter { get; set; } = new();
}

public class LowStockReportFilter
{
    public Guid? BranchId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
}

public enum LowStockOrderKey
{
    QuantityAvailable,
    QuantityOnHand,
    QuantityReserved,
    LowStockThreshold,
    UpdatedAt
}
