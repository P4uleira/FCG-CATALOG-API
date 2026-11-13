using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Abstractions.Search;

public interface IGameSearchService
{
    Task EnsureIndexAsync(CancellationToken cancellationToken);
    Task IndexAsync(Game game, CancellationToken cancellationToken);
    Task<int> SyncAllAsync(IReadOnlyCollection<Game> games, CancellationToken cancellationToken);
    Task<IReadOnlyList<GameSearchResultDto>> SearchAsync(string term, int size, CancellationToken cancellationToken);
}
