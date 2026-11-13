using FCG.Catalog.Application.Abstractions.Search;
using FCG.Catalog.Application.Commands.ReindexGames;
using MediatR;

namespace FCG.Catalog.Api.BackgroundServices;

public sealed class SearchIndexInitializer : BackgroundService
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SearchIndexInitializer> _logger;

    public SearchIndexInitializer(
        IServiceScopeFactory scopeFactory,
        ILogger<SearchIndexInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var searchService = scope.ServiceProvider.GetRequiredService<IGameSearchService>();
                await searchService.EnsureIndexAsync(stoppingToken);

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var indexed = await mediator.Send(new ReindexGamesCommand(), stoppingToken);

                _logger.LogInformation("Índice de busca pronto. {Count} jogos sincronizados.", indexed);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Falha ao preparar o índice de busca. Tentativa {Attempt}/{MaxAttempts}.",
                    attempt,
                    MaxAttempts);

                if (attempt < MaxAttempts)
                    await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }
}
