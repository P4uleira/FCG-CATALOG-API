using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Application.Abstractions.Caching;
using MediatR;

namespace FCG.Catalog.Application.Commands.DeleteGame;

public class DeleteGameCommandHandler : IRequestHandler<DeleteGameCommand, bool>
{
    private readonly IGameRepository _gameRepository;
    private readonly ICacheService _cacheService;

    public DeleteGameCommandHandler(IGameRepository gameRepository, ICacheService cacheService)
    {
        _gameRepository = gameRepository;
        _cacheService = cacheService;   
    }

    public async Task<bool> Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.Id, cancellationToken);

        if (game is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        game.Disable();

        await _gameRepository.UpdateAsync(game, cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.AllGames, cancellationToken);

        return true;
    }
}