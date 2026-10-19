using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Pos.ReportingService.Infrastructure.Shared
{
    public static class ServiceRegistrations
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
