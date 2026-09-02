namespace FCG.Catalog.Application.DTOs;

public sealed record PurchaseHistoryDto(
    Guid OrderId,
    Guid GameId,
    decimal Price,
    string Status,
    DateTime ProcessedAt,
    DateTime RegisteredAt);