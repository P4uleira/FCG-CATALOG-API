using FCG.Catalog.Application.Commands.CompletePurchase;
using FCG.Payments.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Consumers;

public sealed class PaymentProcessedEventConsumer
    : IConsumer<PaymentProcessedEvent>
{
    private readonly IMediator _mediator;

    private readonly ILogger<PaymentProcessedEventConsumer> _logger;

    public PaymentProcessedEventConsumer(
        IMediator mediator,
        ILogger<PaymentProcessedEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "PaymentProcessedEvent recebido. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, Price: {Price}, Status: {Status}, ProcessedAt: {ProcessedAt}",
            message.OrderId,
            message.UserId,
            message.GameId,
            message.Price,
            message.Status,
            message.ProcessedAt);

        var command = new CompletePurchaseCommand(
            message.OrderId,
            message.UserId,
            message.GameId,
            message.Price,
            message.Status,
            message.ProcessedAt);

        var result = await _mediator.Send(
            command,
            context.CancellationToken);

        switch (result.Status)
        {
            case CompletePurchaseStatus.Added:
                _logger.LogInformation(
                    "PaymentProcessedEvent processado com sucesso. Jogo adicionado à biblioteca. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                    result.OrderId,
                    result.UserId,
                    result.GameId);
                break;

            case CompletePurchaseStatus.AlreadyExists:
                _logger.LogInformation(
                    "PaymentProcessedEvent processado com sucesso. Compra já existente e nenhuma duplicidade foi criada. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                    result.OrderId,
                    result.UserId,
                    result.GameId);
                break;

            case CompletePurchaseStatus.PaymentRejected:
                _logger.LogWarning(
                    "PaymentProcessedEvent processado. Pagamento rejeitado e o jogo não foi adicionado. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                    result.OrderId,
                    result.UserId,
                    result.GameId);
                break;

            default:
                throw new InvalidOperationException(
                    $"Resultado de processamento desconhecido: {result.Status}.");
        }
    }
}