namespace FCG.Catalog.Domain.Entities;

public class PurchaseHistoryEntry
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public decimal Price { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTime ProcessedAt { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private PurchaseHistoryEntry()
    {
    }

    private PurchaseHistoryEntry(
        Guid orderId,
        Guid userId,
        Guid gameId,
        decimal price,
        string status,
        DateTime processedAt)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        UserId = userId;
        GameId = gameId;
        Price = price;
        Status = status;
        ProcessedAt = processedAt;
        RegisteredAt = DateTime.UtcNow;
    }

    public static PurchaseHistoryEntry Create(
        Guid orderId,
        Guid userId,
        Guid gameId,
        decimal price,
        string status,
        DateTime processedAt)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException(
                "O identificador do pedido é obrigatório.",
                nameof(orderId));

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "O identificador do usuário é obrigatório.",
                nameof(userId));

        if (gameId == Guid.Empty)
            throw new ArgumentException(
                "O identificador do jogo é obrigatório.",
                nameof(gameId));

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException(
                "O status do pagamento é obrigatório.",
                nameof(status));

        return new PurchaseHistoryEntry(
            orderId,
            userId,
            gameId,
            price,
            status,
            processedAt);
    }

    public static PurchaseHistoryEntry Reconstruct(
    Guid id,
    Guid orderId,
    Guid userId,
    Guid gameId,
    decimal price,
    string status,
    DateTime processedAt,
    DateTime registeredAt)
    {
        return new PurchaseHistoryEntry
        {
            Id = id,
            OrderId = orderId,
            UserId = userId,
            GameId = gameId,
            Price = price,
            Status = status,
            ProcessedAt = processedAt,
            RegisteredAt = registeredAt
        };
    }
}