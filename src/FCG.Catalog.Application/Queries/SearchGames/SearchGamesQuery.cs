using FCG.Catalog.Application.DTOs;
using MediatR;

namespace FCG.Catalog.Application.Queries.SearchGames;

public class SearchGamesQuery : IRequest<IReadOnlyList<GameSearchResultDto>>
{
    public string Term { get; set; } = string.Empty;
    public int Size { get; set; } = 10;
}
