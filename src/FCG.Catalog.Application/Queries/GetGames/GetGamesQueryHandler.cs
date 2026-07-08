using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;

namespace FCG.Catalog.Application.Queries.GetGames;

public class GetGamesQueryHandler : IRequestHandler<GetGamesQuery, IReadOnlyList<GameDto>>
{
    private readonly IGameRepository _gameRepository;

    public GetGamesQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<IReadOnlyList<GameDto>> Handle(GetGamesQuery request, CancellationToken cancellationToken)
    {
        var games = await _gameRepository.GetAllAsync(cancellationToken);

        return games.Select(game => new GameDto
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
    }
}