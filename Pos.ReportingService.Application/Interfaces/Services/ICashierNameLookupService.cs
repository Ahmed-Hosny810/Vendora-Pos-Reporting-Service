namespace Pos.ReportingService.Application.Interfaces.Services;

public interface ICashierNameLookupService
{
    Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> cashierUserIds,
        CancellationToken cancellationToken);
}
