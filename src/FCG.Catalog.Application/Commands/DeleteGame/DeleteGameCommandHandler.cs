using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Application.Abstractions.Caching;
using FCG.Catalog.Application.Abstractions.Search;
using MediatR;

namespace FCG.Catalog.Application.Commands.DeleteGame;

public class DeleteGameCommandHandler : IRequestHandler<DeleteGameCommand, bool>
{
    private readonly IGameRepository _gameRepository;
    private readonly ICacheService _cacheService;
    private readonly IGameSearchService _searchService;

    public DeleteGameCommandHandler(IGameRepository gameRepository, ICacheService cacheService, IGameSearchService searchService)
    {
        _gameRepository = gameRepository;
        _cacheService = cacheService;
        _searchService = searchService;
    }

    public async Task<bool> Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.Id, cancellationToken);

        if (game is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        game.Disable();

        await _gameRepository.UpdateAsync(game, cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.AllGames, cancellationToken);

        await _searchService.IndexAsync(game, cancellationToken);

        return true;
    }
}