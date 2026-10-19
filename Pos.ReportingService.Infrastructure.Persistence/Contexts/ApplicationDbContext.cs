using Microsoft.EntityFrameworkCore;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.ReportingService.Domain.Models;
using Pos.ReportingService.Infrastructure.Persistence.ReadModels;

namespace Pos.ReportingService.Infrastructure.Persistence.Contexts;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;
    private Guid? CurrentTenantId => _currentUser.TenantId;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<SaleReadModel> Sales => Set<SaleReadModel>();
    public DbSet<SaleItemReadModel> SaleItems => Set<SaleItemReadModel>();
    public DbSet<SalePaymentReadModel> SalePayments => Set<SalePaymentReadModel>();
    public DbSet<SaleReturnReadModel> Returns => Set<SaleReturnReadModel>();
    public DbSet<SaleReturnItemReadModel> ReturnItems => Set<SaleReturnItemReadModel>();
    public DbSet<RefundPaymentReadModel> RefundPayments => Set<RefundPaymentReadModel>();
    public DbSet<CashierShiftReadModel> CashierShifts => Set<CashierShiftReadModel>();
    public DbSet<StockBalanceReadModel> StockBalances => Set<StockBalanceReadModel>();
    public DbSet<LowStockAlertReadModel> LowStockAlerts => Set<LowStockAlertReadModel>();
    public DbSet<InventorySnapshot> InventorySnapshots => Set<InventorySnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Missing tenant context returns no data. Background jobs must use explicit tenant scoping.
        modelBuilder.Entity<SaleReadModel>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<SaleItemReadModel>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<SalePaymentReadModel>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<SaleReturnReadModel>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<SaleReturnItemReadModel>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<RefundPaymentReadModel>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<CashierShiftReadModel>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<StockBalanceReadModel>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<LowStockAlertReadModel>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<InventorySnapshot>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
    }
}
