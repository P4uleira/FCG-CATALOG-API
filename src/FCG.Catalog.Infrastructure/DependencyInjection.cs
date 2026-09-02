using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Infrastructure.Data;
using FCG.Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace FCG.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<FCGCatalogDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));

        services.AddScoped<IGameRepository, GameRepository>();

        services.AddScoped<IUserLibraryRepository, UserLibraryRepository>();

        var mongoConnectionString = configuration["Mongo:ConnectionString"];

        if (string.IsNullOrWhiteSpace(mongoConnectionString))
        {
            throw new InvalidOperationException(
                "A configuracao Mongo:ConnectionString nao foi encontrada.");
        }

        var mongoDatabaseName = configuration["Mongo:Database"];

        if (string.IsNullOrWhiteSpace(mongoDatabaseName))
        {
            throw new InvalidOperationException(
                "A configuracao Mongo:Database nao foi encontrada.");
        }

        services.AddSingleton<IMongoClient>(
            _ => new MongoClient(mongoConnectionString));

        services.AddSingleton(serviceProvider =>
        {
            var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
            return mongoClient.GetDatabase(mongoDatabaseName);
        });

        services.AddScoped<IPurchaseHistoryRepository, PurchaseHistoryRepository>();

        return services;
    }
}