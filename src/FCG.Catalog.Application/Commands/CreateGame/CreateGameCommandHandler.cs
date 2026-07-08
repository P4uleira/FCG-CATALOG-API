using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;

namespace FCG.Catalog.Application.Commands.CreateGame;

public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, GameDto>
{
    private readonly IGameRepository _gameRepository;

    public CreateGameCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
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