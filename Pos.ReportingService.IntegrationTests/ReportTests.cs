using Microsoft.EntityFrameworkCore;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;
using Pos.ReportingService.Infrastructure.Persistence.Contexts;

namespace Pos.ReportingService.IntegrationTests;

public class ReportTests
{
    internal static SaleReadModel Sale(TestFixture f, decimal total = 100, Guid? branch = null) => new()
    { Id = Guid.NewGuid(), TenantId = f.TenantId, BranchId = branch ?? f.BranchId, Status = "Completed", CurrencyCode = "EGP", Total = total, CompletedAt = new DateTime(2026,10,10,12,0,0,DateTimeKind.Utc) };
    internal static StockBalanceReadModel Stock(TestFixture f, decimal quantity = 5, Guid? variant = null) => new()
    { Id = Guid.NewGuid(), TenantId = f.TenantId, BranchId = f.BranchId, ProductId = f.ProductId, ProductVariantId = variant, QuantityOnHand = quantity, QuantityReserved = 2, LowStockThreshold = 5 };

    [Fact]
    public async Task Inventory_OptionalFilters_PageTotals_Names_AndTenantIsolation()
    {
        using var f = new TestFixture();
        var other = Stock(f); other.TenantId = Guid.NewGuid();
        await f.SeedAsync(Stock(f,3), Stock(f,7,Guid.NewGuid()), other,
            new BranchReadModel { Id=f.BranchId,TenantId=f.TenantId,NameEn="Main" },
            new ProductReadModel { Id=f.ProductId,TenantId=f.TenantId,NameEn="Coffee" });
        var result = await f.SendAsync(new GetCurrentInventoryReportQuery { Parameter = new() { PageSize=1 } });
        Assert.True(result.IsSuccess); Assert.Equal(2,result.Value!.TotalCount);
        var row = Assert.Single(result.Value.Data); Assert.Equal(1,row.QuantityAvailable); Assert.Equal("Coffee",row.ProductName);
        var empty = await f.SendAsync(new GetCurrentInventoryReportQuery { Parameter = new() { PageNumber=9,PageSize=1 } });
        Assert.Empty(empty.Value!.Data); Assert.Equal(2,empty.Value.TotalCount);
        var filtered = await f.SendAsync(new GetCurrentInventoryReportQuery { Parameter = new() { Filter=new() { BranchId=Guid.NewGuid() } } });
        Assert.Empty(filtered.Value!.Data);
        f.User.TenantId=null;
        Assert.True((await f.SendAsync(new GetCurrentInventoryReportQuery())).IsFailure);
        Assert.Equal(0,await f.WithAsync<ApplicationDbContext,int>(db=>db.StockBalances.CountAsync()));
    }

    [Fact]
    public async Task LowStock_UsesOnHandInclusiveThreshold_ExactVariant_ActiveAlertsOnly()
    {
        using var f = new TestFixture(); var variant=Guid.NewGuid(); var alert=Guid.NewGuid();
        await f.SeedAsync(Stock(f,5),Stock(f,4,variant),Stock(f,6,Guid.NewGuid()),
            new LowStockAlertReadModel { Id=alert,TenantId=f.TenantId,BranchId=f.BranchId,ProductId=f.ProductId,ProductVariantId=variant,Status="Active",DetectedAt=DateTime.UtcNow },
            new LowStockAlertReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,BranchId=f.BranchId,ProductId=f.ProductId,Status="Resolved" });
        var result=await f.SendAsync(new GetLowStockReportQuery());
        Assert.Equal(2,result.Value!.TotalCount);
        Assert.Null(result.Value.Data.Single(x=>x.ProductVariantId==null).ActiveAlertId);
        Assert.Equal(alert,result.Value.Data.Single(x=>x.ProductVariantId==variant).ActiveAlertId);
    }

    [Fact]
    public async Task Dashboard_UsesIssuedSales_ExclusiveDates_Currency_AndReturnDate()
    {
        using var f=new TestFixture();var sale=Sale(f);var draft=Sale(f,900);draft.Status="Draft";
        var usd=Sale(f,500);usd.CurrencyCode="USD";var foreign=Sale(f,700);foreign.TenantId=Guid.NewGuid();
        var end=Sale(f,400);end.CompletedAt=sale.CompletedAt!.Value.AddDays(1);
        await f.SeedAsync(sale,draft,usd,foreign,end,new SaleReturnReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,OriginalSaleId=sale.Id,BranchId=f.BranchId,RefundAmount=20,CompletedAt=sale.CompletedAt.Value });
        var result=await f.SendAsync(new GetSalesDashboardQuery { Filter=new() { FromUtc=sale.CompletedAt,ToUtcExclusive=end.CompletedAt } });
        Assert.Equal(100,result.Value!.SalesTotal);Assert.Equal(20,result.Value.AcceptedReturnAmount);Assert.Equal(80,result.Value.SalesTotalAfterReturns);
    }

    [Fact]
    public async Task ProductAndBranch_IncludeReturnOnlyGroups_AndCorrectReceivingBranch()
    {
        using var f=new TestFixture();var sale=Sale(f);sale.CompletedAt=sale.CompletedAt!.Value.AddMonths(-1);
        var returnId=Guid.NewGuid();var item=Guid.NewGuid();
        await f.SeedAsync(sale,new SaleReturnReadModel { Id=returnId,TenantId=f.TenantId,OriginalSaleId=sale.Id,BranchId=f.BranchId,CompletedAt=new DateTime(2026,10,10),RefundAmount=30 },
            new SaleReturnItemReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,ReturnId=returnId,OriginalSaleId=sale.Id,OriginalSaleItemId=item,ProductId=f.ProductId,Quantity=1,RefundAmount=30 });
        var products=await f.SendAsync(new GetProductSalesReportQuery { Parameter=new() { Filter=new() { FromUtc=new DateTimeOffset(2026,10,1,0,0,0,TimeSpan.Zero) } } });
        var row=Assert.Single(products.Value!.Data);Assert.Equal(-1,row.NetQuantitySold);Assert.Equal(-30,row.SalesTotalAfterReturns);
        var branches=await f.SendAsync(new GetBranchSalesReportQuery { Parameter=new() { Filter=new() { FromUtc=new DateTimeOffset(2026,10,1,0,0,0,TimeSpan.Zero) } } });
        Assert.Equal(-30,Assert.Single(branches.Value!.Data).SalesTotalAfterReturns);
    }

    [Fact]
    public async Task Cashier_GroupsMultipleShiftsForSameCashier_NotSaleCreator()
    {
        using var f=new TestFixture();var cashier=Guid.NewGuid();var shift1=Guid.NewGuid();var shift2=Guid.NewGuid();var a=Sale(f);var b=Sale(f,200);a.ShiftId=shift1;b.ShiftId=shift2;
        await f.SeedAsync(a,b,new CashierShiftReadModel { Id=shift1,TenantId=f.TenantId,CashierUserId=cashier },new CashierShiftReadModel { Id=shift2,TenantId=f.TenantId,CashierUserId=cashier });
        var result=await f.SendAsync(new GetCashierSalesReportQuery());var row=Assert.Single(result.Value!.Data);
        Assert.Equal(cashier,row.CashierUserId);Assert.Equal(2,row.CompletedSaleCount);Assert.Equal(300,row.SalesTotal);
    }

    [Fact]
    public async Task PaymentMethods_UsePaidDate_RetainedAmount_AndRefundPayingBranch()
    {
        using var f=new TestFixture();var sale=Sale(f);sale.Status="PendingPayment";var method=Guid.NewGuid();var ret=Guid.NewGuid();var shift=Guid.NewGuid();var otherBranch=Guid.NewGuid();
        await f.SeedAsync(sale,new SalePaymentReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,SaleId=sale.Id,PaymentMethodId=method,Amount=120,ChangeAmount=20,Status="Completed",PaidAt=new DateTime(2026,10,12) },
            new SalePaymentReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,SaleId=sale.Id,PaymentMethodId=method,Amount=900,Status="Pending",PaidAt=new DateTime(2026,10,12) },
            new SaleReturnReadModel { Id=ret,TenantId=f.TenantId,OriginalSaleId=sale.Id },
            new CashierShiftReadModel { Id=shift,TenantId=f.TenantId,BranchId=otherBranch },
            new RefundPaymentReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,ReturnId=ret,ShiftId=shift,PaymentMethodId=method,Amount=25,Status="Completed",PaidAt=new DateTime(2026,10,12) });
        var result=await f.SendAsync(new GetPaymentMethodReportQuery());var row=Assert.Single(result.Value!.Data);
        Assert.Equal(100,row.CollectedAmount);Assert.Equal(25,row.RefundAmount);Assert.Equal(75,row.NetCollectedAmount);
        var branch=await f.SendAsync(new GetPaymentMethodReportQuery { Parameter=new() { Filter=new() { BranchId=otherBranch } } });
        Assert.Equal(-25,Assert.Single(branch.Value!.Data).NetCollectedAmount);
    }

    [Fact]
    public async Task ReturnReport_IncludesLaterRefunds_AndFiltersSettlementState()
    {
        using var f=new TestFixture();var sale=Sale(f);var ret=Guid.NewGuid();
        await f.SeedAsync(sale,new SaleReturnReadModel { Id=ret,TenantId=f.TenantId,OriginalSaleId=sale.Id,BranchId=f.BranchId,RefundAmount=100,CompletedAt=new DateTime(2026,10,10) },
            new RefundPaymentReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,ReturnId=ret,Amount=40,Status="Completed",PaidAt=new DateTime(2026,11,1) },
            new RefundPaymentReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,ReturnId=ret,Amount=60,Status="Pending" });
        var result=await f.SendAsync(new GetReturnsRefundsReportQuery { Parameter=new() { Filter=new() { ToUtcExclusive=new DateTimeOffset(2026,10,11,0,0,0,TimeSpan.Zero),RefundState="PartiallyRefunded" } } });
        var row=Assert.Single(result.Value!.Data);Assert.Equal(40,row.RefundedAmount);Assert.Equal(60,row.RemainingRefundAmount);
    }

    [Fact]
    public async Task ShiftReport_PreservesEmptyShifts_CashOnly_AndSavedClosedValues()
    {
        using var f=new TestFixture();var shift=Guid.NewGuid();var closed=Guid.NewGuid();var sale=Sale(f);sale.ShiftId=shift;sale.Status="PendingPayment";
        await f.SeedAsync(sale,new CashierShiftReadModel { Id=shift,TenantId=f.TenantId,BranchId=f.BranchId,Status="Open",OpeningCash=50 },
            new CashierShiftReadModel { Id=closed,TenantId=f.TenantId,Status="Closed",ExpectedCash=123,ClosingCash=120,Difference=-3 },
            new SalePaymentReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,SaleId=sale.Id,Status="Completed",IsCashSnapshot=true,Amount=120,ChangeAmount=20 },
            new SalePaymentReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,SaleId=sale.Id,Status="Completed",IsCashSnapshot=false,Amount=900 },
            new RefundPaymentReadModel { Id=Guid.NewGuid(),TenantId=f.TenantId,ShiftId=shift,Status="Completed",IsCashSnapshot=true,Amount=10 });
        var result=await f.SendAsync(new GetShiftReconciliationReportQuery());Assert.Equal(2,result.Value!.TotalCount);
        var open=result.Value.Data.Single(x=>x.ShiftId==shift);Assert.Equal(140,open.ExpectedCash);Assert.Null(open.Difference);
        var saved=result.Value.Data.Single(x=>x.ShiftId==closed);Assert.Equal(123,saved.ExpectedCash);Assert.Equal(-3,saved.Difference);
    }
}
