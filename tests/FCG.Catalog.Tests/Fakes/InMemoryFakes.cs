using System.Text.Json;
using FCG.Catalog.Application.Abstractions.Caching;
using FCG.Catalog.Application.Abstractions.Search;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces.Repositories;

namespace FCG.Catalog.Tests.Fakes;

/// <summary>Repositório em memória para testar os handlers sem banco de dados.</summary>
public class InMemoryGameRepository : IGameRepository
{
    public List<Game> Games { get; } = new();
    public int GetAllCalls { get; private set; }

    public Task<Game> CreateAsync(Game game, CancellationToken cancellationToken)
    {
        Games.Add(game);
        return Task.FromResult(game);
    }

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => Task.FromResult(Games.FirstOrDefault(g => g.Id == id));

    public Task<IReadOnlyList<Game>> GetAllAsync(CancellationToken cancellationToken)
    {
        GetAllCalls++;
        return Task.FromResult<IReadOnlyList<Game>>(Games.ToList());
    }

    public Task UpdateAsync(Game game, CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>Cache em memória que imita o comportamento do Redis (serializa em JSON).</summary>
public class InMemoryCacheService : ICacheService
{
    private readonly Dictionary<string, string> _store = new();

    public List<string> RemovedKeys { get; } = new();

    public bool Contains(string key) => _store.ContainsKey(key);

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        => Task.FromResult(_store.TryGetValue(key, out var json)
            ? JsonSerializer.Deserialize<T>(json)
            : default);

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken)
    {
        _store[key] = JsonSerializer.Serialize(value);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        _store.Remove(key);
        RemovedKeys.Add(key);
        return Task.CompletedTask;
    }
}

/// <summary>Motor de busca falso: registra o que foi indexado/buscado.</summary>
public class InMemoryGameSearchService : IGameSearchService
{
    public List<Game> Indexed { get; } = new();
    public IReadOnlyCollection<Game>? LastSync { get; private set; }
    public string? LastTerm { get; private set; }
    public int? LastSize { get; private set; }
    public List<GameSearchResultDto> Results { get; } = new();

    public Task EnsureIndexAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task IndexAsync(Game game, CancellationToken cancellationToken)
    {
        Indexed.Add(game);
        return Task.CompletedTask;
    }

    public Task<int> SyncAllAsync(IReadOnlyCollection<Game> games, CancellationToken cancellationToken)
    {
        LastSync = games;
        return Task.FromResult(games.Count);
    }

    public Task<IReadOnlyList<GameSearchResultDto>> SearchAsync(string term, int size, CancellationToken cancellationToken)
    {
        LastTerm = term;
        LastSize = size;
        return Task.FromResult<IReadOnlyList<GameSearchResultDto>>(Results);
    }
}
