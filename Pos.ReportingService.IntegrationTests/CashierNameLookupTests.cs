using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.ExportCashierSalesReportQuery;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.IntegrationTests;

public class FakeIdentityHandler : HttpMessageHandler
{
    public Dictionary<Guid, string> Names { get; } = new();
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public int Calls { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Calls++;
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/api/v1/tenant-users/names", request.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
        Assert.Equal("test-user-token", request.Headers.Authorization.Parameter);
        var body = await request.Content!.ReadFromJsonAsync<JsonElement>(ct);
        Assert.False(body.TryGetProperty("tenantId", out _));
        var ids = body.GetProperty("userIds").EnumerateArray().Select(x => x.GetGuid()).ToArray();
        Assert.True(ids.Length <= 500);
        Assert.Equal(ids.Length, ids.Distinct().Count());
        return new HttpResponseMessage(Status)
        {
            Content = JsonContent.Create(new { Succeeded = true, Data = Names.Where(x => ids.Contains(x.Key)).ToDictionary() })
        };
    }
}

public class CashierNameLookupTests
{
    [Fact]
    public async Task Lookup_BatchesIds_ForwardsToken_AndRejectsMismatchedTenant()
    {
        using var f = new TestFixture();
        var ids = Enumerable.Range(0, 501).Select(_ => Guid.NewGuid()).ToArray();
        foreach (var id in ids) f.Identity.Names[id] = "Ahmed";
        var names = await f.WithAsync<ICashierNameLookupService, IReadOnlyDictionary<Guid,string>>(
            s => s.GetNamesAsync(f.TenantId, ids.Concat(ids).ToArray(), default));
        Assert.Equal(501, names.Count);
        Assert.Equal(2, f.Identity.Calls);
        var denied = await f.WithAsync<ICashierNameLookupService, IReadOnlyDictionary<Guid,string>>(
            s => s.GetNamesAsync(Guid.NewGuid(), ids, default));
        Assert.Empty(denied);
        Assert.Equal(2, f.Identity.Calls);
    }

    [Fact]
    public async Task Lookup_IdentityFailure_ReturnsNoNames()
    {
        using var f = new TestFixture();
        f.Identity.Status = HttpStatusCode.ServiceUnavailable;
        var names = await f.WithAsync<ICashierNameLookupService, IReadOnlyDictionary<Guid,string>>(
            s => s.GetNamesAsync(f.TenantId, new[] { Guid.NewGuid() }, default));
        Assert.Empty(names);
    }

    [Fact]
    public async Task CashierReport_AndExport_IncludeNames_WithoutDroppingUnknownCashiers()
    {
        using var f = new TestFixture();
        var cashier = Guid.NewGuid(); var unknown = Guid.NewGuid();
        var firstShift = Guid.NewGuid(); var secondShift = Guid.NewGuid();
        f.Identity.Names[cashier] = "Ahmed Hosney";
        await f.SeedAsync(
            new CashierShiftReadModel { Id = firstShift, TenantId = f.TenantId, CashierUserId = cashier },
            new CashierShiftReadModel { Id = secondShift, TenantId = f.TenantId, CashierUserId = unknown },
            new SaleReadModel { Id = Guid.NewGuid(), TenantId = f.TenantId, ShiftId = firstShift, Status = "Completed", CurrencyCode = "EGP", Total = 100, CompletedAt = DateTime.UtcNow },
            new SaleReadModel { Id = Guid.NewGuid(), TenantId = f.TenantId, ShiftId = secondShift, Status = "Completed", CurrencyCode = "EGP", Total = 50, CompletedAt = DateTime.UtcNow });
        var report = await f.SendAsync(new GetCashierSalesReportQuery());
        Assert.Equal(2, report.Value!.TotalCount);
        Assert.Equal("Ahmed Hosney", report.Value.Data.Single(x => x.CashierUserId == cashier).CashierName);
        Assert.Null(report.Value.Data.Single(x => x.CashierUserId == unknown).CashierName);
        Assert.Equal(150, report.Value.Data.Sum(x => x.SalesTotal));
        var export = await f.SendAsync(new ExportCashierSalesReportQuery());
        Assert.Contains("Ahmed Hosney", Encoding.UTF8.GetString(export.Value!.Content));
        Assert.Contains(unknown.ToString(), Encoding.UTF8.GetString(export.Value.Content));
    }
}
