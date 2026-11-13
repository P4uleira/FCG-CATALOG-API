using FCG.Catalog.Application.Abstractions.Caching;
using FCG.Catalog.Application.Commands.CreateGame;
using FCG.Catalog.Application.Commands.UpdateGame;
using FCG.Catalog.Application.Queries.GetGameById;
using FCG.Catalog.Application.Queries.GetGames;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace FCG.Catalog.Tests.Application;

public class CreateGameCommandHandlerTests
{
    private readonly InMemoryGameRepository _repository = new();
    private readonly InMemoryCacheService _cache = new();
    private readonly InMemoryGameSearchService _search = new();

    [Fact]
    public async Task Handle_ShouldCreateGame_AndInvalidateCache()
    {
        var handler = new CreateGameCommandHandler(_repository, _cache, _search);

        var result = await handler.Handle(new CreateGameCommand
        {
            Title = "Hollow Knight",
            Description = "Metroidvania",
            Price = 49.90m,
            Genre = "Aventura"
        }, CancellationToken.None);

        Assert.Single(_repository.Games);
        Assert.Equal("Hollow Knight", result.Title);
        Assert.True(result.Active);
        Assert.Contains(CacheKeys.AllGames, _cache.RemovedKeys);
        Assert.Single(_search.Indexed);
        Assert.Equal(result.Id, _search.Indexed[0].Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldThrow_WhenTitleIsBlank(string title)
    {
        var handler = new CreateGameCommandHandler(_repository, _cache, _search);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new CreateGameCommand { Title = title, Price = 10m }, CancellationToken.None));

        Assert.Empty(_repository.Games);
        Assert.Empty(_search.Indexed);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPriceIsNegative()
    {
        var handler = new CreateGameCommandHandler(_repository, _cache, _search);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new CreateGameCommand { Title = "Jogo", Price = -1m }, CancellationToken.None));

        Assert.Empty(_repository.Games);
    }
}

public class UpdateGameCommandHandlerTests
{
    private readonly InMemoryGameRepository _repository = new();
    private readonly InMemoryCacheService _cache = new();
    private readonly InMemoryGameSearchService _search = new();

    [Fact]
    public async Task Handle_ShouldUpdateGame_AndInvalidateCache()
    {
        var game = new Game("Titulo", "Descricao", 10m, "Acao");
        _repository.Games.Add(game);
        var handler = new UpdateGameCommandHandler(_repository, _cache, _search);

        var result = await handler.Handle(
            new UpdateGameCommand { Id = game.Id, Price = 30m }, CancellationToken.None);

        Assert.Equal(30m, result.Price);
        Assert.Equal("Titulo", result.Title);
        Assert.Contains(CacheKeys.AllGames, _cache.RemovedKeys);
        Assert.Single(_search.Indexed);
        Assert.Equal(30m, _search.Indexed[0].Price);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenGameDoesNotExist()
    {
        var handler = new UpdateGameCommandHandler(_repository, _cache, _search);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(
            new UpdateGameCommand { Id = Guid.NewGuid(), Price = 30m }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPriceIsNegative()
    {
        var game = new Game("Titulo", "Descricao", 10m, "Acao");
        _repository.Games.Add(game);
        var handler = new UpdateGameCommandHandler(_repository, _cache, _search);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new UpdateGameCommand { Id = game.Id, Price = -5m }, CancellationToken.None));

        Assert.Equal(10m, game.Price);
    }
}

public class GetGameByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnGame_WhenItExists()
    {
        var repository = new InMemoryGameRepository();
        var game = new Game("Celeste", "Plataforma", 36.99m, "Plataforma");
        repository.Games.Add(game);
        var handler = new GetGameByIdQueryHandler(repository);

        var result = await handler.Handle(new GetGameByIdQuery { Id = game.Id }, CancellationToken.None);

        Assert.Equal(game.Id, result.Id);
        Assert.Equal("Celeste", result.Title);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenGameDoesNotExist()
    {
        var handler = new GetGameByIdQueryHandler(new InMemoryGameRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetGameByIdQuery { Id = Guid.NewGuid() }, CancellationToken.None));
    }
}

public class GetGamesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldQueryRepository_AndFillCache_OnCacheMiss()
    {
        var repository = new InMemoryGameRepository();
        repository.Games.Add(new Game("Jogo A", "", 10m, "Acao"));
        var cache = new InMemoryCacheService();
        var handler = new GetGamesQueryHandler(repository, cache, NullLogger<GetGamesQueryHandler>.Instance);

        var result = await handler.Handle(new GetGamesQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(1, repository.GetAllCalls);
        Assert.True(cache.Contains(CacheKeys.AllGames));
    }

    [Fact]
    public async Task Handle_ShouldNotQueryRepository_OnCacheHit()
    {
        var repository = new InMemoryGameRepository();
        repository.Games.Add(new Game("Jogo A", "", 10m, "Acao"));
        var cache = new InMemoryCacheService();
        var handler = new GetGamesQueryHandler(repository, cache, NullLogger<GetGamesQueryHandler>.Instance);

        await handler.Handle(new GetGamesQuery(), CancellationToken.None); // MISS -> preenche o cache
        var result = await handler.Handle(new GetGamesQuery(), CancellationToken.None); // HIT

        Assert.Single(result);
        Assert.Equal(1, repository.GetAllCalls);
    }
}
