using System.Text.Json;
using FCG.Catalog.Application.Abstractions.Caching;
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
