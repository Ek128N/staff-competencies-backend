using Microsoft.EntityFrameworkCore;
using staff_competencies_backend.Services;
using staff_competencies_backend.Storage;

namespace staff_competencies_backend.Utils;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IPersonService, PersonService>();

        services.AddDbContext<CompetenciesDbContext>(options =>
            {
                var connectionString = config.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString);
            })
            .MigrateDatabase();
        return services;
    }

    private static IServiceCollection MigrateDatabase(this IServiceCollection services)
    {
        using var scope = services.BuildServiceProvider().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CompetenciesDbContext>();
        context.Database.Migrate();
        return services;
    }
}