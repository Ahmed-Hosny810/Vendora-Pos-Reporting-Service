using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.ExportCurrentInventoryReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;
using Pos.ReportingService.Infrastructure.Shared.Options;
using Pos.ReportingService.Infrastructure.Shared.Services;
using Pos.ReportingService.WebApi.Policies;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.IntegrationTests;

public class ExportAndValidationTests
{
    [Theory]
    [InlineData("SalesReports","ExportSalesDashboardReportQuery")]
    [InlineData("SalesReports","ExportProductSalesReportQuery")]
    [InlineData("SalesReports","ExportBranchSalesReportQuery")]
    [InlineData("SalesReports","ExportCashierSalesReportQuery")]
    [InlineData("SalesReports","ExportPaymentMethodReportQuery")]
    [InlineData("SalesReports","ExportReturnsRefundsReportQuery")]
    [InlineData("SalesReports","ExportShiftReconciliationReportQuery")]
    [InlineData("InventoryReports","ExportLowStockReportQuery")]
    [InlineData("InventoryReports","ExportCurrentInventoryReportQuery")]
    public async Task ExportQueries_AreRegistered_AllowEmptyFilters_AndGenerateFiles(string group,string name)
    {
        using var f=new TestFixture();
        var type=typeof(ExportCurrentInventoryReportQuery).Assembly.GetType($"Pos.ReportingService.Application.Features.{group}.Queries.{name}.{name}")!;
        var result=(Result<ReportExportResult>)(await f.SendObjectAsync(Activator.CreateInstance(type)!))!;
        Assert.True(result.IsSuccess);Assert.EndsWith(".csv",result.Value!.FileName);
        Assert.Equal("text/csv; charset=utf-8",result.Value.ContentType);
        Assert.True(result.Value.Content.AsSpan().StartsWith(new byte[] {239,187,191}));
        Assert.Contains("\r\n",Encoding.UTF8.GetString(result.Value.Content));
    }

    [Fact]
    public async Task Export_ContainsAllRows_NotJustFirstPage_AndExcludesOtherTenants()
    {
        using var f=new TestFixture();
        var balances=Enumerable.Range(0,55).Select(i=>(object)ReportTests.Stock(f,i,Guid.NewGuid())).ToList();
        var foreign=ReportTests.Stock(f);foreign.TenantId=Guid.NewGuid();balances.Add(foreign);
        balances.Add(new ProductReadModel { Id=f.ProductId,TenantId=f.TenantId,NameEn="قهوة, \"Blue\"" });
        await f.SeedAsync(balances.ToArray());
        var result=await f.SendAsync(new ExportCurrentInventoryReportQuery());
        Assert.True(result.IsSuccess);
        var csv=Encoding.UTF8.GetString(result.Value!.Content);
        Assert.Equal(56,csv.Split("\r\n",StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.Contains("قهوة",csv);
    }

    [Fact]
    public async Task Export_RejectsOverLimit_AndHonorsFilter()
    {
        using var f=new TestFixture(1);var variant=Guid.NewGuid();
        await f.SeedAsync(ReportTests.Stock(f),ReportTests.Stock(f,5,variant));
        Assert.True((await f.SendAsync(new ExportCurrentInventoryReportQuery())).IsFailure);
        var filtered=await f.SendAsync(new ExportCurrentInventoryReportQuery { Filter=new() { ProductVariantId=variant } });
        Assert.True(filtered.IsSuccess);
    }

    [Fact]
    public async Task Mediator_RejectsInvalidFilters_BeforeQueryExecution()
    {
        using var f=new TestFixture();
        await Assert.ThrowsAsync<Pos.ReportingService.Application.Exceptions.ValidationException>(()=>f.SendAsync(
            new GetSalesDashboardQuery { Filter=new() { FromUtc=DateTimeOffset.UtcNow,ToUtcExclusive=DateTimeOffset.UtcNow.AddDays(-1) } }));
        await Assert.ThrowsAsync<Pos.ReportingService.Application.Exceptions.ValidationException>(()=>f.SendAsync(
            new ExportCurrentInventoryReportQuery { OrderKey=(Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery.CurrentInventoryOrderKey)999 }));
    }

    [Fact]
    public void Csv_EscapesText_UsesInvariantNumbers_BomAndDelimiterOptions_AndCancellation()
    {
        var service=new CsvExportService(Options.Create(new CsvExportOptions { Delimiter=";",IncludeUtf8Bom=false }));
        var previous=System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture=new System.Globalization.CultureInfo("fr-FR");
            var content=service.Write(["Text","Number"],new IReadOnlyList<object?>[] { new object?[] { "=1+1",12.5m },new object?[] { "a;\"b\"\nق",-2m } },default);
            var csv=Encoding.UTF8.GetString(content);
            Assert.False(content.AsSpan().StartsWith(new byte[] {239,187,191}));
            Assert.Contains("\"'=1+1\";\"12.5\"",csv);
            Assert.Contains("\"a;\"\"b\"\"\nق\"",csv);
            Assert.Contains("\"-2\"",csv);
            Assert.Throws<OperationCanceledException>(()=>service.Write(["A"],new IReadOnlyList<object?>[]{new object?[]{1}},new CancellationToken(true)));
        }
        finally { System.Globalization.CultureInfo.CurrentCulture=previous; }
    }

    [Theory]
    [InlineData("TenantOwner",true,true)]
    [InlineData("Admin",true,true)]
    [InlineData("Cashier",true,false)]
    [InlineData("TenantOwner",false,false)]
    public async Task RealReportPolicy_RequiresRoleAndCurrentSubscription(string role,bool active,bool allowed)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddAppPolicies();
        using var provider=services.BuildServiceProvider();
        var principal=new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.Role,role),new Claim("sub",Guid.NewGuid().ToString()),new Claim("tenant_id",Guid.NewGuid().ToString()),
            new Claim("user_type","Tenant"),new Claim("business_access","true"),
            new Claim("subscription_access_valid_until",DateTimeOffset.UtcNow.AddHours(active?1:-1).ToString("O")),
            new Claim("subscription_ends_at",DateTimeOffset.UtcNow.AddDays(1).ToString("O")) },"Test"));
        var result=await provider.GetRequiredService<IAuthorizationService>().AuthorizeAsync(principal,null,AppPolicies.ViewReports);
        Assert.Equal(allowed,result.Succeeded);
    }
}
