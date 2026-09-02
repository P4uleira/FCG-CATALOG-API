using FCG.Catalog.Application.Abstractions.Caching;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Queries.GetGames;

public class GetGamesQueryHandler : IRequestHandler<GetGamesQuery, IReadOnlyList<GameDto>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    private readonly IGameRepository _gameRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetGamesQueryHandler> _logger;

    public GetGamesQueryHandler(
        IGameRepository gameRepository,
        ICacheService cacheService,
        ILogger<GetGamesQueryHandler> logger)
    {
        _gameRepository = gameRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GameDto>> Handle(GetGamesQuery request, CancellationToken cancellationToken)
    {
        var cachedGames = await _cacheService.GetAsync<List<GameDto>>(
            CacheKeys.AllGames,
            cancellationToken);

        if (cachedGames is not null)
        {
            _logger.LogInformation("Cache HIT para {CacheKey}", CacheKeys.AllGames);
            return cachedGames;
        }

        _logger.LogInformation("Cache MISS para {CacheKey}. Consultando SQL Server.", CacheKeys.AllGames);

        var games = await _gameRepository.GetAllAsync(cancellationToken);

        var gameDtos = games.Select(game => new GameDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Genre = game.Genre,
            Active = game.Active,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt
        }).ToList();

        await _cacheService.SetAsync(CacheKeys.AllGames, gameDtos, CacheDuration, cancellationToken);

        return gameDtos;
    }
}