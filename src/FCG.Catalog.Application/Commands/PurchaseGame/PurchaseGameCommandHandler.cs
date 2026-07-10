using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Contracts.Events;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Commands.PurchaseGame;

public sealed class PurchaseGameCommandHandler
    : IRequestHandler<PurchaseGameCommand, PurchaseGameResponse>
{
    private readonly IGameRepository _gameRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PurchaseGameCommandHandler> _logger;

    public PurchaseGameCommandHandler(
        IGameRepository gameRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<PurchaseGameCommandHandler> logger)
    {
        _gameRepository = gameRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<PurchaseGameResponse> Handle(
        PurchaseGameCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Compra iniciada. UserId: {UserId}, GameId: {GameId}",
            request.UserId,
            request.GameId);

        var game = await _gameRepository.GetByIdAsync(
            request.GameId,
            cancellationToken);

        if (game is null)
        {
            _logger.LogWarning(
                "Compra rejeitada: jogo inexistente. UserId: {UserId}, GameId: {GameId}",
                request.UserId,
                request.GameId);

            throw new KeyNotFoundException(
                $"Jogo com o identificador '{request.GameId}' não foi encontrado.");
        }

        if (!game.Active)
        {
            _logger.LogWarning(
                "Compra rejeitada: jogo inativo. UserId: {UserId}, GameId: {GameId}",
                request.UserId,
                request.GameId);

            throw new InvalidOperationException(
                $"O jogo '{game.Title}' está inativo e não pode ser comprado.");
        }

        var orderId = Guid.NewGuid();
        var purchasedAt = DateTime.UtcNow;

        var orderPlacedEvent = new OrderPlacedEvent(
            orderId,
            request.UserId,
            game.Id,
            game.Price,
            purchasedAt);

        await _publishEndpoint.Publish(
            orderPlacedEvent,
            cancellationToken);

        _logger.LogInformation(
            "Evento OrderPlacedEvent publicado. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, Price: {Price}",
            orderId,
            request.UserId,
            game.Id,
            game.Price);

        return new PurchaseGameResponse(
            orderId,
            request.UserId,
            game.Id,
            game.Price,
            purchasedAt,
            "Pending");
    }
}