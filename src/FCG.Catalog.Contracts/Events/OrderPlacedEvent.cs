namespace FCG.Catalog.Contracts.Events;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    DateTime PurchasedAt);