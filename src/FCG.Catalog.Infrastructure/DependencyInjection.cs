using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Infrastructure.Data;
using FCG.Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using FCG.Catalog.Infrastructure.Caching;
using FCG.Catalog.Infrastructure.Search;
using FCG.Catalog.Application.Abstractions.Search;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

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

        services.AddStackExchangeRedisCache(options =>
        {
            var redisConnectionString = configuration["Redis:ConnectionString"];

            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                throw new InvalidOperationException(
                    "A configuracao Redis:ConnectionString nao foi encontrada.");
            }

            options.Configuration = redisConnectionString;
            options.InstanceName = "fcg-catalog:";
        });

        services.AddScoped<FCG.Catalog.Application.Abstractions.Caching.ICacheService, RedisCacheService>();

        AddSearch(services, configuration);

        return services;
    }

    private static void AddSearch(IServiceCollection services, IConfiguration configuration)
    {
        var elasticsearchUrl = configuration["Elasticsearch:Url"];

        if (string.IsNullOrWhiteSpace(elasticsearchUrl))
        {
            services.AddSingleton<IGameSearchService, NoOpGameSearchService>();
            return;
        }

        var apiKey = configuration["Elasticsearch:ApiKey"];
        var indexName = configuration["Elasticsearch:IndexName"];

        if (string.IsNullOrWhiteSpace(indexName))
            indexName = "fcg-games";

        services.AddSingleton<IGameSearchService>(serviceProvider =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(elasticsearchUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(10)
            };

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("ApiKey", apiKey);
            }

            return new ElasticGameSearchService(
                httpClient,
                indexName.ToLowerInvariant(),
                serviceProvider.GetRequiredService<ILogger<ElasticGameSearchService>>());
        });
    }
}
