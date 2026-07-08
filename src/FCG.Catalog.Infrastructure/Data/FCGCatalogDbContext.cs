using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Data;

public class FCGCatalogDbContext : DbContext
{
    public FCGCatalogDbContext(DbContextOptions<FCGCatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FCGCatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}