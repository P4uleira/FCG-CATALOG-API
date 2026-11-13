using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Application.Abstractions.Caching;
using FCG.Catalog.Application.Abstractions.Search;
using MediatR;

namespace FCG.Catalog.Application.Commands.CreateGame;

public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, GameDto>
{
    private readonly IGameRepository _gameRepository;
    private readonly ICacheService _cacheService;
    private readonly IGameSearchService _searchService;

    public CreateGameCommandHandler(IGameRepository gameRepository, ICacheService cacheService, IGameSearchService searchService)
    {
        _gameRepository = gameRepository;
        _cacheService = cacheService;
        _searchService = searchService;
    }

    public async Task<GameDto> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("O título do jogo é obrigatório.");

        if (request.Price < 0)
            throw new ArgumentException("O preço do jogo não pode ser negativo.");

        var game = new Game(
            request.Title,
            request.Description,
            request.Price,
            request.Genre
        );

        await _gameRepository.CreateAsync(game, cancellationToken);

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