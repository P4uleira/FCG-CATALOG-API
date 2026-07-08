using FCG.Catalog.Application.DTOs;
using MediatR;

namespace FCG.Catalog.Application.Queries.GetGameById;

public class GetGameByIdQuery : IRequest<GameDto>
{
    public Guid Id { get; set; }
}