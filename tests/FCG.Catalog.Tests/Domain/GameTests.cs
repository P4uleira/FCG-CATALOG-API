using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Tests.Domain;

public class GameTests
{
    [Fact]
    public void Constructor_ShouldCreateActiveGame_WhenDataIsValid()
    {
        var game = new Game("Elden Ring", "RPG de ação", 199.90m, "RPG");

        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.Equal("Elden Ring", game.Title);
        Assert.Equal("RPG de ação", game.Description);
        Assert.Equal(199.90m, game.Price);
        Assert.Equal("RPG", game.Genre);
        Assert.True(game.Active);
        Assert.True(game.CreatedAt <= DateTime.UtcNow);
        Assert.Null(game.UpdatedAt);
    }

    [Fact]
    public void Update_ShouldReplaceAllFields_AndSetUpdatedAt()
    {
        var game = new Game("Titulo", "Descricao", 10m, "Acao");

        game.Update("Novo titulo", "Nova descricao", 20m, "Aventura");

        Assert.Equal("Novo titulo", game.Title);
        Assert.Equal("Nova descricao", game.Description);
        Assert.Equal(20m, game.Price);
        Assert.Equal("Aventura", game.Genre);
        Assert.NotNull(game.UpdatedAt);
    }

    [Fact]
    public void UpdatePartial_ShouldChangeOnlyProvidedFields()
    {
        var game = new Game("Titulo", "Descricao", 10m, "Acao");

        game.UpdatePartial(title: null, description: null, price: 50m, genre: null, active: null);

        Assert.Equal("Titulo", game.Title);
        Assert.Equal("Descricao", game.Description);
        Assert.Equal(50m, game.Price);
        Assert.Equal("Acao", game.Genre);
        Assert.True(game.Active);
        Assert.NotNull(game.UpdatedAt);
    }

    [Fact]
    public void UpdatePartial_ShouldIgnoreBlankTitleAndGenre()
    {
        var game = new Game("Titulo", "Descricao", 10m, "Acao");

        game.UpdatePartial(title: "   ", description: null, price: null, genre: "", active: null);

        Assert.Equal("Titulo", game.Title);
        Assert.Equal("Acao", game.Genre);
    }

    [Fact]
    public void UpdatePartial_ShouldAllowDeactivatingGame()
    {
        var game = new Game("Titulo", "Descricao", 10m, "Acao");

        game.UpdatePartial(null, null, null, null, active: false);

        Assert.False(game.Active);
    }

    [Fact]
    public void Disable_ShouldDeactivateGame_AndSetUpdatedAt()
    {
        var game = new Game("Titulo", "Descricao", 10m, "Acao");

        game.Disable();

        Assert.False(game.Active);
        Assert.NotNull(game.UpdatedAt);
    }
}
