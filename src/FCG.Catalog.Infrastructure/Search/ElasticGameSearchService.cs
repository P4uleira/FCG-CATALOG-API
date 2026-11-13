using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FCG.Catalog.Application.Abstractions.Search;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Infrastructure.Search;

/// <summary>
/// Implementação do motor de busca usando a API REST do Elasticsearch
/// (Elastic Cloud). A busca é totalmente independente do SQL Server.
/// </summary>
public sealed class ElasticGameSearchService : IGameSearchService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string _indexName;
    private readonly ILogger<ElasticGameSearchService> _logger;

    public ElasticGameSearchService(
        HttpClient httpClient,
        string indexName,
        ILogger<ElasticGameSearchService> logger)
    {
        _httpClient = httpClient;
        _indexName = indexName;
        _logger = logger;
    }

    public async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        using var headRequest = new HttpRequestMessage(HttpMethod.Head, _indexName);
        using var headResponse = await _httpClient.SendAsync(headRequest, cancellationToken);

        if (headResponse.IsSuccessStatusCode)
            return;

        if (headResponse.StatusCode != HttpStatusCode.NotFound)
            headResponse.EnsureSuccessStatusCode();

        var indexDefinition = new
        {
            settings = new
            {
                analysis = new
                {
                    analyzer = new
                    {
                        fcg_text = new
                        {
                            type = "custom",
                            tokenizer = "standard",
                            filter = new[] { "lowercase", "asciifolding" }
                        }
                    }
                }
            },
            mappings = new
            {
                properties = new
                {
                    id = new { type = "keyword" },
                    title = new { type = "text", analyzer = "fcg_text", fields = new { keyword = new { type = "keyword" } } },
                    description = new { type = "text", analyzer = "fcg_text" },
                    genre = new { type = "text", analyzer = "fcg_text", fields = new { keyword = new { type = "keyword" } } },
                    price = new { type = "double" },
                    active = new { type = "boolean" },
                    createdAt = new { type = "date" },
                    updatedAt = new { type = "date" }
                }
            }
        };

        using var createResponse = await _httpClient.PutAsJsonAsync(
            _indexName, indexDefinition, JsonOptions, cancellationToken);

        if (createResponse.IsSuccessStatusCode)
        {
            _logger.LogInformation("Índice {IndexName} criado no Elasticsearch.", _indexName);
            return;
        }

        var error = await createResponse.Content.ReadAsStringAsync(cancellationToken);

        if (error.Contains("resource_already_exists_exception", StringComparison.Ordinal))
            return;

        throw new HttpRequestException(
            $"Falha ao criar o índice {_indexName}: {(int)createResponse.StatusCode} {error}");
    }

    public async Task IndexAsync(Game game, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PutAsJsonAsync(
                $"{_indexName}/_doc/{game.Id}?refresh=true",
                GameSearchDocument.From(game),
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Falha ao indexar o jogo {GameId}: {StatusCode} {Error}",
                    game.Id, (int)response.StatusCode, error);
                return;
            }

            _logger.LogInformation("Jogo {GameId} indexado no Elasticsearch.", game.Id);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Falha ao indexar o jogo {GameId} no Elasticsearch.", game.Id);
        }
    }

    public async Task<int> SyncAllAsync(IReadOnlyCollection<Game> games, CancellationToken cancellationToken)
    {
        if (games.Count > 0)
        {
            var bulkBody = new StringBuilder();

            foreach (var game in games)
            {
                bulkBody.AppendLine(JsonSerializer.Serialize(
                    new { index = new { _index = _indexName, _id = game.Id } }, JsonOptions));
                bulkBody.AppendLine(JsonSerializer.Serialize(GameSearchDocument.From(game), JsonOptions));
            }

            using var bulkContent = new StringContent(bulkBody.ToString(), Encoding.UTF8, "application/x-ndjson");
            using var bulkResponse = await _httpClient.PostAsync("_bulk?refresh=true", bulkContent, cancellationToken);
            var bulkResult = await bulkResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!bulkResponse.IsSuccessStatusCode)
                throw new HttpRequestException($"Falha no bulk do Elasticsearch: {(int)bulkResponse.StatusCode} {bulkResult}");

            using var bulkJson = JsonDocument.Parse(bulkResult);
            if (bulkJson.RootElement.GetProperty("errors").GetBoolean())
                throw new HttpRequestException($"O bulk do Elasticsearch retornou erros: {bulkResult}");
        }

        // Remove do índice os documentos que não existem mais no banco principal
        var deleteStale = new
        {
            query = new
            {
                @bool = new
                {
                    must_not = new object[]
                    {
                        new { ids = new { values = games.Select(g => g.Id.ToString()).ToArray() } }
                    }
                }
            }
        };

        using var deleteResponse = await _httpClient.PostAsJsonAsync(
            $"{_indexName}/_delete_by_query?refresh=true&conflicts=proceed",
            deleteStale,
            JsonOptions,
            cancellationToken);

        if (!deleteResponse.IsSuccessStatusCode)
        {
            var error = await deleteResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Falha ao limpar o índice: {(int)deleteResponse.StatusCode} {error}");
        }

        _logger.LogInformation("Índice {IndexName} sincronizado com {Count} jogos.", _indexName, games.Count);

        return games.Count;
    }

    public async Task<IReadOnlyList<GameSearchResultDto>> SearchAsync(
        string term,
        int size,
        CancellationToken cancellationToken)
    {
        var searchRequest = new
        {
            size,
            query = new
            {
                @bool = new
                {
                    must = new
                    {
                        multi_match = new
                        {
                            query = term,
                            fields = new[] { "title^3", "genre^2", "description" },
                            fuzziness = "AUTO",
                            prefix_length = 0,
                            max_expansions = 50
                        }
                    },
                    filter = new { term = new { active = true } }
                }
            },
            // Ordenação por relevância (maior pontuação primeiro)
            sort = new object[] { new { _score = new { order = "desc" } } }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{_indexName}/_search", searchRequest, JsonOptions, cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Falha na busca do Elasticsearch: {(int)response.StatusCode} {body}");

        using var json = JsonDocument.Parse(body);
        var hits = json.RootElement.GetProperty("hits").GetProperty("hits");

        var results = new List<GameSearchResultDto>();

        foreach (var hit in hits.EnumerateArray())
        {
            var document = hit.GetProperty("_source").Deserialize<GameSearchDocument>(JsonOptions);

            if (document is null)
                continue;

            var score = hit.TryGetProperty("_score", out var scoreElement) && scoreElement.ValueKind == JsonValueKind.Number
                ? scoreElement.GetDouble()
                : 0d;

            results.Add(new GameSearchResultDto
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                Price = document.Price,
                Genre = document.Genre,
                Score = score
            });
        }

        return results;
    }

    private sealed class GameSearchDocument
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public static GameSearchDocument From(Game game) => new()
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Genre = game.Genre,
            Price = game.Price,
            Active = game.Active,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt
        };
    }
}
