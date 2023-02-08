using Application.Interfaces;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEventManagerService, EventManagerService>();
        
        services.AddDbContext<MusicDbContext>(option =>
        {
            var dbName = configuration["MusicDbName"];
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = "music471omni.db3";
            }

            var path = Path.Combine(PathHelper.DatabasePath, dbName);
            option.UseSqlite($"Data Source={path}");
        });
    
        services.AddDbContext<CardDbContext>(option =>
        {
            var dbName = configuration["CardDbName"];
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = "card.db3";
            }

            var path = Path.Combine(PathHelper.DatabasePath, dbName);
            option.UseSqlite($"Data Source={path}");
        });

        services.AddScoped<ICardDbContext>(provider => provider.GetService<CardDbContext>() ?? throw new InvalidOperationException());
        services.AddScoped<IMusicDbContext>(provider => provider.GetService<MusicDbContext>() ?? throw new InvalidOperationException());
        
        return services;
    }
}