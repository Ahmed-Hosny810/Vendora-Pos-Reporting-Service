using Pos.ReportingService.Application;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.ReportingService.Infrastructure.Persistence;
using Pos.ReportingService.Infrastructure.Shared;
using Pos.ReportingService.WebApi.Extensions;
using Pos.ReportingService.WebApi.Services;
using Serilog;

namespace Pos.ReportingService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            });

            // API Versioning
            builder.Services.AddApiVersioningExtension();


            //-------------------Services Registration-----------------------

            builder.Services.AddPersistenceServices(builder.Configuration);

            builder.Services.AddSharedInfrastructure(builder.Configuration);

            builder.Services.AddApplicationLayer(builder.Configuration);

            builder.Services.AddAuthenticationServices(builder.Configuration);

            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.AddControllers();

            // Swagger (via extension)
            builder.Services.AddSwaggerExtension();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerExtension();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
