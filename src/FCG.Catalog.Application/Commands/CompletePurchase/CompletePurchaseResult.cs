namespace FCG.Catalog.Application.Commands.CompletePurchase;

public enum CompletePurchaseStatus
{
    Added,
    AlreadyExists,
    PaymentRejected
}

public sealed record CompletePurchaseResult(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    CompletePurchaseStatus Status);