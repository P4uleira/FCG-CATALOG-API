using MediatR;

namespace FCG.Catalog.Application.Commands.CompletePurchase;

public sealed record CompletePurchaseCommand(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    string Status,
    DateTime ProcessedAt)
    : IRequest<CompletePurchaseResult>;