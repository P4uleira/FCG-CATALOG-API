using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Data.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(g => g.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(g => g.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(g => g.Genre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Active)
            .IsRequired();

        builder.Property(g => g.CreatedAt)
            .IsRequired();

        builder.Property(g => g.UpdatedAt);

        builder.HasIndex(g => g.Title);
    }
}