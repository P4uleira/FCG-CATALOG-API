using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public class UserLibraryRepository : IUserLibraryRepository
{
    private readonly FCGCatalogDbContext _context;

    public UserLibraryRepository(
        FCGCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(
        Guid userId,
        Guid gameId,
        CancellationToken cancellationToken)
    {
        return await _context.UserLibrary
            .AsNoTracking()
            .AnyAsync(
                userLibrary =>
                    userLibrary.UserId == userId &&
                    userLibrary.GameId == gameId,
                cancellationToken);
    }

    public async Task AddAsync(
        UserLibrary userLibrary,
        CancellationToken cancellationToken)
    {
        await _context.UserLibrary.AddAsync(
            userLibrary,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<Game>> GetGamesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await (
            from userLibrary in _context.UserLibrary.AsNoTracking()
            join game in _context.Games.AsNoTracking()
                on userLibrary.GameId equals game.Id
            where userLibrary.UserId == userId
            orderby userLibrary.PurchasedAt descending
            select game
        ).ToListAsync(cancellationToken);
    }
}