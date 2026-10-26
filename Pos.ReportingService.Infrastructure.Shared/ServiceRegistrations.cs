using Pos.ReportingService.Application.Interfaces.Services;
using Pos.ReportingService.Infrastructure.Shared.Options;
using Pos.ReportingService.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Pos.ReportingService.Infrastructure.Shared
{
    public static class ServiceRegistrations
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<CsvExportOptions>()
                .Bind(configuration.GetSection("CsvExport"))
                .Validate(options => options.MaxRows >= 1 && options.MaxRows <= 100000,
                    "CsvExport:MaxRows must be between 1 and 100000.")
                .Validate(options => options.Delimiter == "," || options.Delimiter == ";",
                    "CsvExport:Delimiter must be a comma or semicolon.")
                .ValidateOnStart();
            services.AddScoped<ICsvExportService, CsvExportService>();
            return services;
        }
    }
}
