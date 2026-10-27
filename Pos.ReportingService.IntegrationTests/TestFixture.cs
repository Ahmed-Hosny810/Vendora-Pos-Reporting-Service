using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.ReportingService.Application;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.ReportingService.Infrastructure.Persistence;
using Pos.ReportingService.Infrastructure.Persistence.Contexts;
using Pos.ReportingService.Infrastructure.Shared;

namespace Pos.ReportingService.IntegrationTests;

public sealed class TestFixture : IDisposable
{
    public Guid TenantId { get; } = Guid.NewGuid();
    public Guid BranchId { get; } = Guid.NewGuid();
    public Guid ProductId { get; } = Guid.NewGuid();
    public FakeCurrentUser User { get; } = new();
    public FakeIdentityHandler Identity { get; } = new();
    public ServiceProvider Services { get; }
    public bool IsSqlServer { get; }
    private readonly string _databaseName = "ReportingIntegrationTests_" + Guid.NewGuid().ToString("N");

    public TestFixture(int maxRows = 100)
    {
        var server = Environment.GetEnvironmentVariable("REPORTING_TEST_SQL_CONNECTION");
        IsSqlServer = !string.IsNullOrWhiteSpace(server);
        // Never use the supplied database. Every fixture owns a new disposable database.
        var connection = IsSqlServer ? new SqlConnectionStringBuilder(server)
            { InitialCatalog = _databaseName, AttachDBFilename = "", Pooling = false }.ConnectionString : null;
        User.TenantId = TenantId;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connection,
            ["Reporting:BusinessTimeZoneId"] = "Africa/Cairo",
            ["CsvExport:MaxRows"] = maxRows.ToString()
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationLayer(configuration);
        services.AddPersistenceServices(configuration);
        services.AddSharedInfrastructure(configuration);
        services.AddSingleton<ICurrentUserService>(User);
        services.AddScoped<ICashierNameLookupService>(sp =>
            new Pos.ReportingService.Infrastructure.Shared.Services.CashierNameLookupService(
                new HttpClient(Identity, false) { BaseAddress = new Uri("https://identity.test/") },
                User, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Pos.ReportingService.Infrastructure.Shared.Services.CashierNameLookupService>>()));
        services.AddScoped<ApplicationDbContext>(sp => new TestContext(IsSqlServer
            ? new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection).Options
            : new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(_databaseName).Options, User));
        Services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    public async Task SeedAsync(params object[] rows)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.AddRange(rows);
        await db.SaveChangesAsync();
    }
    public async Task<T> SendAsync<T>(IRequest<T> query)
    {
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(query);
    }
    public async Task<object?> SendObjectAsync(object query)
    {
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(query);
    }
    public async Task<T> WithAsync<TService,T>(Func<TService,Task<T>> action) where TService : notnull
    {
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }
    public void Dispose()
    {
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (IsSqlServer && new SqlConnectionStringBuilder(db.Database.GetConnectionString()).InitialCatalog != _databaseName)
                throw new InvalidOperationException("Refusing to delete an unexpected database.");
            db.Database.EnsureDeleted();
        }
        Services.Dispose();
    }
}

// Test-only writable representations of source tables. Production models remain keyless/read-only.
internal sealed class TestContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService user)
    : ApplicationDbContext(options, user)
{
    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        foreach (var type in model.Model.GetEntityTypes().Where(x => x.IsKeyless).ToList())
        {
            var table = type.GetViewName(); var schema = type.GetViewSchema();
            var entity = model.Entity(type.ClrType);
            entity.HasKey("Id"); entity.ToView(null); entity.ToTable(table!, schema);
            entity.Property("Id").ValueGeneratedNever();
        }
    }
}
public sealed class FakeCurrentUser : ICurrentUserService
{
    public Guid? TenantId { get; set; }
    public string? UserId => "a51d63c9-4d32-49cc-a41e-11ff72bfd499";
    public string? UserType => "Tenant";
    public IReadOnlyList<string> Roles => ["TenantOwner"];
    public string? AccessToken => "test-user-token";
}
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REPORTING_TEST_SQL_CONNECTION")))
            Skip = "Requires REPORTING_TEST_SQL_CONNECTION; AT TIME ZONE cannot execute in EF InMemory.";
    }
}
