using FCG.Catalog.Application.Abstractions.Search;
using FCG.Catalog.Application.DTOs;
using MediatR;

namespace FCG.Catalog.Application.Queries.SearchGames;

public class SearchGamesQueryHandler : IRequestHandler<SearchGamesQuery, IReadOnlyList<GameSearchResultDto>>
{
    public const int MaxSize = 50;

    private readonly IGameSearchService _searchService;

    public SearchGamesQueryHandler(IGameSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<IReadOnlyList<GameSearchResultDto>> Handle(
        SearchGamesQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Term))
            throw new ArgumentException("Informe o termo de busca (parâmetro q).");

        var size = Math.Clamp(request.Size, 1, MaxSize);

        return await _searchService.SearchAsync(request.Term.Trim(), size, cancellationToken);
    }
}
