using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Tests.Domain;

public class UserLibraryTests
{
    [Fact]
    public void Create_ShouldCreateLibraryEntry_WhenDataIsValid()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var purchasedAt = DateTime.UtcNow;

        var library = UserLibrary.Create(userId, gameId, purchasedAt);

        Assert.NotEqual(Guid.Empty, library.Id);
        Assert.Equal(userId, library.UserId);
        Assert.Equal(gameId, library.GameId);
        Assert.Equal(purchasedAt, library.PurchasedAt);
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            UserLibrary.Create(Guid.Empty, Guid.NewGuid(), DateTime.UtcNow));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenGameIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            UserLibrary.Create(Guid.NewGuid(), Guid.Empty, DateTime.UtcNow));

        Assert.Equal("gameId", exception.ParamName);
    }
}

public class PurchaseHistoryEntryTests
{
    [Fact]
    public void Create_ShouldCreateEntry_WhenDataIsValid()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var processedAt = DateTime.UtcNow;

        var entry = PurchaseHistoryEntry.Create(orderId, userId, gameId, 99.90m, "Approved", processedAt);

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(orderId, entry.OrderId);
        Assert.Equal(userId, entry.UserId);
        Assert.Equal(gameId, entry.GameId);
        Assert.Equal(99.90m, entry.Price);
        Assert.Equal("Approved", entry.Status);
        Assert.Equal(processedAt, entry.ProcessedAt);
    }

    [Theory]
    [InlineData("orderId")]
    [InlineData("userId")]
    [InlineData("gameId")]
    public void Create_ShouldThrow_WhenAnyIdIsEmpty(string emptyParam)
    {
        var orderId = emptyParam == "orderId" ? Guid.Empty : Guid.NewGuid();
        var userId = emptyParam == "userId" ? Guid.Empty : Guid.NewGuid();
        var gameId = emptyParam == "gameId" ? Guid.Empty : Guid.NewGuid();

        var exception = Assert.Throws<ArgumentException>(() =>
            PurchaseHistoryEntry.Create(orderId, userId, gameId, 10m, "Approved", DateTime.UtcNow));

        Assert.Equal(emptyParam, exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenStatusIsBlank(string status)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            PurchaseHistoryEntry.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, status, DateTime.UtcNow));

        Assert.Equal("status", exception.ParamName);
    }
}
