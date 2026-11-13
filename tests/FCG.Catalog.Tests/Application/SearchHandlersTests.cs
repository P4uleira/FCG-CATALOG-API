using FCG.Catalog.Application.Commands.DeleteGame;
using FCG.Catalog.Application.Commands.ReindexGames;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Queries.SearchGames;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Tests.Fakes;

namespace FCG.Catalog.Tests.Application;

public class SearchGamesQueryHandlerTests
{
    private readonly InMemoryGameSearchService _search = new();

    [Fact]
    public async Task Handle_ShouldReturnResultsFromSearchEngine_InSameOrder()
    {
        _search.Results.Add(new GameSearchResultDto { Title = "The Legend of Zelda", Score = 5.2 });
        _search.Results.Add(new GameSearchResultDto { Title = "Zelda II", Score = 3.1 });
        var handler = new SearchGamesQueryHandler(_search);

        var result = await handler.Handle(new SearchGamesQuery { Term = "  zelad  " }, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("The Legend of Zelda", result[0].Title);
        Assert.Equal("zelad", _search.LastTerm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldThrow_WhenTermIsBlank(string term)
    {
        var handler = new SearchGamesQueryHandler(_search);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(new SearchGamesQuery { Term = term }, CancellationToken.None));

        Assert.Null(_search.LastTerm);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 10)]
    [InlineData(500, SearchGamesQueryHandler.MaxSize)]
    public async Task Handle_ShouldClampSize(int requested, int expected)
    {
        var handler = new SearchGamesQueryHandler(_search);

        await handler.Handle(new SearchGamesQuery { Term = "jogo", Size = requested }, CancellationToken.None);

        Assert.Equal(expected, _search.LastSize);
    }
}

public class ReindexGamesCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSyncAllGamesFromRepository()
    {
        var repository = new InMemoryGameRepository();
        repository.Games.Add(new Game("Jogo A", "", 10m, "Acao"));
        repository.Games.Add(new Game("Jogo B", "", 20m, "RPG"));
        var search = new InMemoryGameSearchService();
        var handler = new ReindexGamesCommandHandler(repository, search);

        var indexed = await handler.Handle(new ReindexGamesCommand(), CancellationToken.None);

        Assert.Equal(2, indexed);
        Assert.NotNull(search.LastSync);
        Assert.Equal(2, search.LastSync!.Count);
    }
}

public class DeleteGameCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDisableGame_AndReindexAsInactive()
    {
        var repository = new InMemoryGameRepository();
        var game = new Game("Jogo", "", 10m, "Acao");
        repository.Games.Add(game);
        var cache = new InMemoryCacheService();
        var search = new InMemoryGameSearchService();
        var handler = new DeleteGameCommandHandler(repository, cache, search);

        var result = await handler.Handle(new DeleteGameCommand { Id = game.Id }, CancellationToken.None);

        Assert.True(result);
        Assert.False(game.Active);
        Assert.Single(search.Indexed);
        Assert.False(search.Indexed[0].Active);
    }
}
