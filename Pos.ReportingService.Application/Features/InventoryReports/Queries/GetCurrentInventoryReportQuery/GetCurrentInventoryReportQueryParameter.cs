using Pos.SalesService.Application.Parameters;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;

public class GetCurrentInventoryReportQueryParameter : RequestParameter<CurrentInventoryOrderKey>
{
    public CurrentInventoryReportFilter Filter { get; set; } = new();
}

public class CurrentInventoryReportFilter
{
    public Guid? BranchId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
}

public enum CurrentInventoryOrderKey
{
    QuantityAvailable,
    QuantityOnHand,
    QuantityReserved,
    LowStockThreshold,
    UpdatedAt
}
