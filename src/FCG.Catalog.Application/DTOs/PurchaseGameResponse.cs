namespace FCG.Catalog.Application.DTOs;

public sealed record PurchaseGameResponse(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    DateTime PurchasedAt,
    string Status);