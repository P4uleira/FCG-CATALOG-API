using FCG.Catalog.Application.DTOs;
using MediatR;

namespace FCG.Catalog.Application.Commands.UpdateGame;

public class UpdateGameCommand : IRequest<GameDto>
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Genre { get; set; }
    public bool? Active { get; set; }
}