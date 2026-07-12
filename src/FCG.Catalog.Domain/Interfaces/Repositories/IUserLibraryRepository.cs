using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Domain.Interfaces.Repositories;

public interface IUserLibraryRepository
{
    Task<bool> ExistsAsync(
        Guid userId,
        Guid gameId,
        CancellationToken cancellationToken);

    Task AddAsync(
        UserLibrary userLibrary,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Game>> GetGamesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}