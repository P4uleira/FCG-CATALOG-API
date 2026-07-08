using FCG.Catalog.Application.DTOs;
using MediatR;

namespace FCG.Catalog.Application.Queries.GetGames;

public class GetGamesQuery : IRequest<IReadOnlyList<GameDto>>
{
}