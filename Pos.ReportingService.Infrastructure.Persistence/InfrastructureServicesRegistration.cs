using Pos.ReportingService.Infrastructure.Persistence.Options;
using Pos.ReportingService.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Infrastructure.Persistence.Contexts;
using Pos.ReportingService.Infrastructure.Persistence.Repositories;
using Pos.SalesService.Application.Interfaces;
using Pos.SalesService.Infrastructure.Persistence.UnitofWork;

namespace Pos.ReportingService.Infrastructure.Persistence
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<ReportingOptions>()
                .Bind(configuration.GetSection("Reporting"))
                .Validate(options => ReportingTimeZone.IsValidId(options.BusinessTimeZoneId),
                    "Reporting:BusinessTimeZoneId must identify a supported named timezone.")
                .ValidateOnStart();
            services.AddSingleton<ReportingTimeZone>();
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => {
                        sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "reporting");
                    }));

            services.AddScoped(typeof(IGenericRepositoryAsync<,>), typeof(GenericRepositoryAsync<,>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ISalesReportRepositoryAsync, SalesReportRepositoryAsync>();
            services.AddScoped<IInventoryReportRepositoryAsync, InventoryReportRepositoryAsync>();

            return services;
        }
    }
}
