using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;

namespace FCG.Catalog.Application.Queries.GetGameById;

public class GetGameByIdQueryHandler : IRequestHandler<GetGameByIdQuery, GameDto>
{
    private readonly IGameRepository _gameRepository;

    public GetGameByIdQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<GameDto> Handle(GetGameByIdQuery request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.Id, cancellationToken);

        if (game is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

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