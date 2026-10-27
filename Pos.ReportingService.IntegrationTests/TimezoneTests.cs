using Microsoft.Extensions.Options;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetDailySalesQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.ExportDailySalesReportQuery;
using Pos.ReportingService.Infrastructure.Persistence.Options;
using Pos.ReportingService.Infrastructure.Persistence.Services;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;
using System.Text;

namespace Pos.ReportingService.IntegrationTests;

public class TimezoneTests
{
    [Fact]
    public void Cairo_All2026DayBoundaries_IncludeDstAndExclusiveEnd()
    {
        var service=new ReportingTimeZone(Options.Create(new ReportingOptions()));
        var zone=TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        var lengths=new HashSet<double>();
        for(var date=new DateOnly(2026,1,1);date<new DateOnly(2027,1,1);date=date.AddDays(1))
        {
            var from=service.StartOfDayUtc(date);var to=service.StartOfDayUtc(date.AddDays(1));lengths.Add((to-from).TotalHours);
            Assert.Equal(date,DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(from,zone).DateTime));
            Assert.Equal(date,DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(to.AddTicks(-1),zone).DateTime));
        }
        Assert.Contains(23d,lengths);Assert.Contains(25d,lengths);
        Assert.Null(service.ToUtcFilter(new DailySalesReportFilter()).FromUtc);
        Assert.False(ReportingTimeZone.IsValidId("Not/AZone"));
    }

    [SqlServerFact]
    [Trait("Category","SqlServer")]
    public async Task DailyReport_AndCsv_GroupByCairoDate_AndUseReturnDate()
    {
        using var f=new TestFixture();
        var sale=ReportTests.Sale(f);sale.CompletedAt=new DateTime(2026,10,10,22,30,0,DateTimeKind.Utc);
        var excluded=ReportTests.Sale(f,500);excluded.CompletedAt=new DateTime(2026,10,11,21,0,0,DateTimeKind.Utc);
        await f.SeedAsync(sale,excluded,new SaleReturnReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,OriginalSaleId=sale.Id,BranchId=f.BranchId,RefundAmount=20,CompletedAt=new DateTime(2026,10,11,0,30,0,DateTimeKind.Utc) });
        var filter=new DailySalesReportFilter { FromDate=new DateOnly(2026,10,11),ToDateExclusive=new DateOnly(2026,10,12) };
        var result=await f.SendAsync(new GetDailySalesReportQuery { Filter=filter });
        var row=Assert.Single(result.Value!);Assert.Equal(new DateOnly(2026,10,11),row.Date);Assert.Equal(100,row.SalesTotal);Assert.Equal(80,row.SalesTotalAfterReturns);
        var csv=await f.SendAsync(new ExportDailySalesReportQuery { Filter=filter });
        Assert.True(csv.IsSuccess);Assert.Contains("2026-10-11",Encoding.UTF8.GetString(csv.Value!.Content));
    }

    [SqlServerFact]
    [Trait("Category","SqlServer")]
    public async Task DailyExport_DetectsOversizedDaysIncludingReturnOnlyDays()
    {
        using var f=new TestFixture(1);var sale=ReportTests.Sale(f);
        await f.SeedAsync(sale,new SaleReturnReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,OriginalSaleId=sale.Id,BranchId=f.BranchId,RefundAmount=10,CompletedAt=sale.CompletedAt!.Value.AddDays(1) });
        Assert.True((await f.SendAsync(new ExportDailySalesReportQuery())).IsFailure);
    }
}
