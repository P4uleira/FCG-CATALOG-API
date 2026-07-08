using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;

namespace FCG.Catalog.Application.Commands.UpdateGame;

public class UpdateGameCommandHandler : IRequestHandler<UpdateGameCommand, GameDto>
{
    private readonly IGameRepository _gameRepository;

    public UpdateGameCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
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