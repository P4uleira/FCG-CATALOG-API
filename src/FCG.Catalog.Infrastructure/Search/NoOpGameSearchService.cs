using FCG.Catalog.Application.Abstractions.Search;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Infrastructure.Search;

/// <summary>
/// Usado quando Elasticsearch:Url não está configurado (ex.: ambiente local sem Elasticsearch).
/// A API continua funcionando; apenas a busca avançada fica indisponível.
/// </summary>
public sealed class NoOpGameSearchService : IGameSearchService
{
    private readonly ILogger<NoOpGameSearchService> _logger;

    public NoOpGameSearchService(ILogger<NoOpGameSearchService> logger)
    {
        _logger = logger;
    }

    public Task EnsureIndexAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task IndexAsync(Game game, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<int> SyncAllAsync(IReadOnlyCollection<Game> games, CancellationToken cancellationToken)
        => Task.FromResult(0);

    public Task<IReadOnlyList<GameSearchResultDto>> SearchAsync(string term, int size, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Busca solicitada, mas o Elasticsearch não está configurado (Elasticsearch:Url).");
        throw new InvalidOperationException("Busca avançada indisponível: Elasticsearch não configurado.");
    }
}
