using System.Text.Json;
using Codeflix.Catalog.Api.Filters;

namespace Codeflix.Catalog.Api.Configurations;

public static class ControllersConfiguration
{
    public static IServiceCollection AddControllersConfiguration(this IServiceCollection services)
    {
        services
            .AddControllers(options => options.Filters.Add(typeof(GlobalExceptionFilter)))
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            );
        services.AddDocumentation();
        return services;
    }

    private static IServiceCollection AddDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static WebApplication UseDocumentation(this WebApplication app)
    {
        if (
            app.Environment.IsDevelopment()
            || app.Environment.EnvironmentName.Equals("Docker", StringComparison.OrdinalIgnoreCase)
        )
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        return app;
    }
}
