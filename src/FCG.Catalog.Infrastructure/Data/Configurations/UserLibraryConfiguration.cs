using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Data.Configurations;

public class UserLibraryConfiguration
    : IEntityTypeConfiguration<UserLibrary>
{
    public void Configure(
        EntityTypeBuilder<UserLibrary> builder)
    {
        builder.ToTable("UserLibrary");

        builder.HasKey(userLibrary => userLibrary.Id);

        builder.Property(userLibrary => userLibrary.Id)
            .ValueGeneratedNever();

        builder.Property(userLibrary => userLibrary.UserId)
            .IsRequired();

        builder.Property(userLibrary => userLibrary.GameId)
            .IsRequired();

        builder.Property(userLibrary => userLibrary.PurchasedAt)
            .IsRequired();

        builder.HasIndex(userLibrary => new
        {
            userLibrary.UserId,
            userLibrary.GameId
        })
            .IsUnique();

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(userLibrary => userLibrary.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}