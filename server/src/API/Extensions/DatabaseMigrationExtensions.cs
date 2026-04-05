using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        const int retryCount = 5;
        var delay = TimeSpan.FromSeconds(2);
        
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("✅ Database migration completed successfully.");
                break;
            }
            catch (Exception ex) when (i < retryCount - 1)
            {
                Console.WriteLine($"⚠️ Migration attempt {i + 1} failed: {ex.Message}. Retrying in {delay.TotalSeconds} seconds...");
                await Task.Delay(delay);
            }
        }
        
        return app;
    }
}