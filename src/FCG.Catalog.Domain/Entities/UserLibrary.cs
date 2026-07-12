namespace FCG.Catalog.Domain.Entities;

public class UserLibrary
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    private UserLibrary()
    {
    }

    private UserLibrary(
        Guid userId,
        Guid gameId,
        DateTime purchasedAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        GameId = gameId;
        PurchasedAt = purchasedAt;
    }

    public static UserLibrary Create(
        Guid userId,
        Guid gameId,
        DateTime purchasedAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "O identificador do usuário é obrigatório.",
                nameof(userId));

        if (gameId == Guid.Empty)
            throw new ArgumentException(
                "O identificador do jogo é obrigatório.",
                nameof(gameId));

        return new UserLibrary(
            userId,
            gameId,
            purchasedAt);
    }
}