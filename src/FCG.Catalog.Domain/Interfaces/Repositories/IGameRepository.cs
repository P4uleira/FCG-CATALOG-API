using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Domain.Interfaces.Repositories;

public interface IGameRepository
{
    Task<Game> CreateAsync(Game game, CancellationToken cancellationToken);
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Game>> GetAllAsync(CancellationToken cancellationToken);
    Task UpdateAsync(Game game, CancellationToken cancellationToken);
}