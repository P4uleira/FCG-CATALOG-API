using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;

namespace FCG.Catalog.Application.Commands.DeleteGame;

public class DeleteGameCommandHandler : IRequestHandler<DeleteGameCommand, bool>
{
    private readonly IGameRepository _gameRepository;

    public DeleteGameCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<bool> Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.Id, cancellationToken);

        if (game is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        game.Disable();

        await _gameRepository.UpdateAsync(game, cancellationToken);

        return true;
    }
}