using Application.Interfaces.Services;
using Infrastructure;
using Infrastructure.Neo4j;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Neo4j.Driver;
using Serilog;

namespace API.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultPGConnection"));
        });

        // Neo4j
        var settings = new Neo4jSettings();
        configuration.GetSection("Neo4jSettings").Bind(settings);
        
        try
        {
            var neo4JDriver = GraphDatabase.Driver(
                settings.Neo4jConnection,
                AuthTokens.Basic(settings.Neo4juser, settings.Neo4jPassword)
            );
            services.AddSingleton(neo4JDriver);
            services.AddTransient<INeo4jDataAccess, Neo4jDataAccess>();
            services.AddScoped<INeo4JSubscriptionService, Neo4JSubscriptionService>();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to initialize Neo4j Driver");
        }

        return services;
    } 
}