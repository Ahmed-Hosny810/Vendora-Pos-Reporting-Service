using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pos.ReportingService.Application.Interfaces.Services;

namespace Pos.ReportingService.Infrastructure.Shared.Services;

public class CashierNameLookupService(
    HttpClient client,
    ICurrentUserService currentUser,
    ILogger<CashierNameLookupService> logger) : ICashierNameLookupService
{
    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(
        Guid tenantId, IReadOnlyCollection<Guid> cashierUserIds, CancellationToken cancellationToken)
    {
        var names = new Dictionary<Guid, string>();
        if (tenantId == Guid.Empty || tenantId != currentUser.TenantId || cashierUserIds.Count == 0)
            return names;

        if (string.IsNullOrWhiteSpace(currentUser.AccessToken))
        {
            logger.LogWarning("Cashier names unavailable: no user access token.");
            return names;
        }

        try
        {
            foreach (var batch in cashierUserIds.Where(id => id != Guid.Empty).Distinct().Chunk(500))
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/tenant-users/names");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentUser.AccessToken);
                request.Content = JsonContent.Create(new { UserIds = batch });
                using var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<NameLookupResponse>(cancellationToken);
                if (result?.Succeeded != true || result.Data == null)
                {
                    logger.LogWarning("Identity returned an unsuccessful cashier name lookup.");
                    return names;
                }

                // Accept only IDs requested in this batch.
                foreach (var id in batch)
                    if (result.Data.TryGetValue(id, out var name))
                        names[id] = name;
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Identity cashier name lookup timed out.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Identity cashier name lookup failed.");
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Identity returned invalid cashier names.");
        }

        return names;
    }

    public class NameLookupResponse
    {
        public bool Succeeded { get; set; }
        public Dictionary<Guid, string>? Data { get; set; }
    }
}
