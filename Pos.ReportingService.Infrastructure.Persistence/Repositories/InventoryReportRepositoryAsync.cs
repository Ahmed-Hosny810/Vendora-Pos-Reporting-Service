using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;
using Pos.ReportingService.Application.Features.InventoryReports.DTOs;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;
using Pos.SalesService.Application.Wrappers;
using Microsoft.EntityFrameworkCore;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Infrastructure.Persistence.Contexts;
using Pos.ReportingService.Infrastructure.Persistence.QueryExtensions;

namespace Pos.ReportingService.Infrastructure.Persistence.Repositories;

public class InventoryReportRepositoryAsync : IInventoryReportRepositoryAsync
{
    private readonly ApplicationDbContext _context;

    public InventoryReportRepositoryAsync(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<IEnumerable<CurrentInventoryReportDto>>> GetCurrentInventoryReportAsync(
        Guid tenantId, GetCurrentInventoryReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        var query = _context.StockBalances
            .AsNoTracking()
            .ApplyInventoryFilter(tenantId, parameter.Filter)
            .Select(balance => new CurrentInventoryReportDto
            {
                StockBalanceId = balance.Id,
                BranchId = balance.BranchId,
                ProductId = balance.ProductId,
                ProductVariantId = balance.ProductVariantId,
                QuantityOnHand = balance.QuantityOnHand,
                QuantityReserved = balance.QuantityReserved,
                QuantityAvailable = balance.QuantityOnHand - balance.QuantityReserved,
                LowStockThreshold = balance.LowStockThreshold,
                UpdatedAt = balance.UpdatedAt
            });

        var offset = ((long)parameter.PageNumber - 1) * parameter.PageSize;

        if (offset > int.MaxValue)
            return new PagedResponse<IEnumerable<CurrentInventoryReportDto>>(
                Array.Empty<CurrentInventoryReportDto>(), parameter.PageNumber, parameter.PageSize,
                await query.CountAsync(cancellationToken));

        var rows = await query.ApplyOrder(parameter)
            .Skip((int)offset)
            .Take(parameter.PageSize)
            .ToListAsync(cancellationToken);

        // Infer the total only from a nonempty short page or an empty first page.
        var totalCountIsKnown = (rows.Count > 0 && rows.Count < parameter.PageSize) ||
            (rows.Count == 0 && offset == 0);
        var totalCount = totalCountIsKnown
            ? checked((int)(offset + rows.Count)) : await query.CountAsync(cancellationToken);

        if (rows.Count > 0)
            await AddNamesAsync(tenantId, rows, cancellationToken);

        return new PagedResponse<IEnumerable<CurrentInventoryReportDto>>(
            rows, parameter.PageNumber, parameter.PageSize, totalCount);
    }

    public async Task<PagedResponse<IEnumerable<LowStockReportDto>>> GetLowStockReportAsync(
        Guid tenantId, GetLowStockReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        var query = _context.StockBalances
            .AsNoTracking()
            .ApplyLowStockFilter(tenantId, parameter.Filter)
            .Select(balance => new LowStockReportDto
            {
                StockBalanceId = balance.Id,
                BranchId = balance.BranchId,
                ProductId = balance.ProductId,
                ProductVariantId = balance.ProductVariantId,
                QuantityOnHand = balance.QuantityOnHand,
                QuantityReserved = balance.QuantityReserved,
                QuantityAvailable = balance.QuantityOnHand - balance.QuantityReserved,
                LowStockThreshold = balance.LowStockThreshold,
                UpdatedAt = balance.UpdatedAt
            });

        var offset = ((long)parameter.PageNumber - 1) * parameter.PageSize;

        if (offset > int.MaxValue)
            return new PagedResponse<IEnumerable<LowStockReportDto>>(
                Array.Empty<LowStockReportDto>(), parameter.PageNumber, parameter.PageSize,
                await query.CountAsync(cancellationToken));

        var rows = await query.ApplyOrder(parameter)
            .Skip((int)offset)
            .Take(parameter.PageSize)
            .ToListAsync(cancellationToken);

        // Infer the total only from a nonempty short page or an empty first page.
        var totalCountIsKnown = (rows.Count > 0 && rows.Count < parameter.PageSize) ||
            (rows.Count == 0 && offset == 0);
        var totalCount = totalCountIsKnown
            ? checked((int)(offset + rows.Count)) : await query.CountAsync(cancellationToken);

        if (rows.Count > 0)
        {
            await AddNamesAsync(tenantId, rows, cancellationToken);
            await AddActiveAlertsAsync(tenantId, rows, cancellationToken);
        }

        return new PagedResponse<IEnumerable<LowStockReportDto>>(
            rows, parameter.PageNumber, parameter.PageSize, totalCount);
    }

    private async Task AddNamesAsync(Guid tenantId, IReadOnlyList<CurrentInventoryReportDto> rows, CancellationToken cancellationToken)
    {
        var branchIds = rows.Select(x => x.BranchId).Distinct().ToList();
        var productIds = rows.Select(x => x.ProductId).Distinct().ToList();
        var variantIds = rows.Where(x => x.ProductVariantId.HasValue)
            .Select(x => x.ProductVariantId!.Value).Distinct().ToList();

        var branches = await _context.Branches
            .Where(x => x.TenantId == tenantId && branchIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.NameEn, cancellationToken);
        var products = await _context.Products
            .Where(x => x.TenantId == tenantId && productIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.NameEn, cancellationToken);
        var variants = variantIds.Count == 0 ? new Dictionary<(Guid, Guid), string>()
            : await _context.ProductVariants
                .Where(x => x.TenantId == tenantId && productIds.Contains(x.ProductId) && variantIds.Contains(x.Id))
                .ToDictionaryAsync(x => (x.ProductId, x.Id), x => x.Name, cancellationToken);

        foreach (var row in rows)
        {
            row.BranchName = branches.GetValueOrDefault(row.BranchId) ?? $"Unavailable branch ({row.BranchId})";
            row.ProductName = products.GetValueOrDefault(row.ProductId) ?? $"Unavailable product ({row.ProductId})";
            row.VariantName = row.ProductVariantId.HasValue
                ? variants.GetValueOrDefault((row.ProductId, row.ProductVariantId.Value))
                    ?? $"Unavailable variant ({row.ProductVariantId})"
                : null;
        }
    }
    private async Task AddActiveAlertsAsync(Guid tenantId, List<LowStockReportDto> rows, CancellationToken cancellationToken)
    {
        var branchIds = rows.Select(x => x.BranchId).Distinct().ToList();
        var productIds = rows.Select(x => x.ProductId).Distinct().ToList();
        var variantIds = rows.Where(x => x.ProductVariantId.HasValue)
            .Select(x => x.ProductVariantId!.Value).Distinct().ToList();
        var includesProductsWithoutVariant = rows.Any(x => !x.ProductVariantId.HasValue);

        var alerts = await _context.LowStockAlerts
            .Where(x => x.TenantId == tenantId &&
                x.Status == ReadModels.Constants.LowStockAlertStatuses.Active &&
                branchIds.Contains(x.BranchId) && productIds.Contains(x.ProductId) &&
                ((!x.ProductVariantId.HasValue && includesProductsWithoutVariant) ||
                 (x.ProductVariantId.HasValue && variantIds.Contains(x.ProductVariantId.Value))))
            .Select(x => new { x.Id, x.BranchId, x.ProductId, x.ProductVariantId, x.DetectedAt })
            .ToListAsync(cancellationToken);

        // Exact keys prevent matching another variant or branch from the batch.
        var lookup = alerts.GroupBy(x => (x.BranchId, x.ProductId, x.ProductVariantId))
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.DetectedAt).ThenBy(x => x.Id).First());
        foreach (var row in rows)
        {
            if (lookup.TryGetValue((row.BranchId, row.ProductId, row.ProductVariantId), out var alert))
            {
                row.ActiveAlertId = alert.Id;
                row.DetectedAt = alert.DetectedAt;
            }
        }
    }
}
