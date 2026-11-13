using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Application.Abstractions.Caching;
using FCG.Catalog.Application.Abstractions.Search;
using MediatR;

namespace FCG.Catalog.Application.Commands.UpdateGame;

public class UpdateGameCommandHandler : IRequestHandler<UpdateGameCommand, GameDto>
{
    private readonly IGameRepository _gameRepository;
    private readonly ICacheService _cacheService;
    private readonly IGameSearchService _searchService;

    public UpdateGameCommandHandler(IGameRepository gameRepository, ICacheService cacheService, IGameSearchService searchService)
    {
        _gameRepository = gameRepository;
        _cacheService = cacheService;
        _searchService = searchService;
    }

    public async Task<GameDto> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.Id, cancellationToken);

        if (game is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        if (request.Price.HasValue && request.Price.Value < 0)
            throw new ArgumentException("O preço do jogo não pode ser negativo.");

        game.UpdatePartial(
            request.Title,
            request.Description,
            request.Price,
            request.Genre,
            request.Active
        );

        await _gameRepository.UpdateAsync(game, cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.AllGames, cancellationToken);

        await _searchService.IndexAsync(game, cancellationToken);

        return new GameDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Genre = game.Genre,
            Active = game.Active,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt
        };
    }
}