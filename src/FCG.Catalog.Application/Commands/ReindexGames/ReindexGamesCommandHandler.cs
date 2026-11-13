using FCG.Catalog.Application.Abstractions.Search;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;

namespace FCG.Catalog.Application.Commands.ReindexGames;

public class ReindexGamesCommandHandler : IRequestHandler<ReindexGamesCommand, int>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameSearchService _searchService;

    public ReindexGamesCommandHandler(IGameRepository gameRepository, IGameSearchService searchService)
    {
        _gameRepository = gameRepository;
        _searchService = searchService;
    }

    public async Task<int> Handle(ReindexGamesCommand request, CancellationToken cancellationToken)
    {
        var games = await _gameRepository.GetAllAsync(cancellationToken);

        return await _searchService.SyncAllAsync(games.ToList(), cancellationToken);
    }
}
