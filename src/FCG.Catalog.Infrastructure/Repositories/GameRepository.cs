using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly FCGCatalogDbContext _context;

    public GameRepository(FCGCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Game> CreateAsync(Game game, CancellationToken cancellationToken)
    {
        await _context.Games.AddAsync(game, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return game;
    }

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Games
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Game>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Games
            .OrderBy(g => g.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Game game, CancellationToken cancellationToken)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync(cancellationToken);
    }
}