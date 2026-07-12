using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Infrastructure.Data;
using FCG.Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FCGCatalogDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "DefaultConnection")));

        services.AddScoped<IGameRepository, GameRepository>();

        services.AddScoped<
            IUserLibraryRepository,
            UserLibraryRepository>();

        return services;
    }
}