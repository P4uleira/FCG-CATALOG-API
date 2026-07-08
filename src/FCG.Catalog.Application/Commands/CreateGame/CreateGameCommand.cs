using FCG.Catalog.Application.DTOs;
using MediatR;

namespace FCG.Catalog.Application.Commands.CreateGame;

public class CreateGameCommand : IRequest<GameDto>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Genre { get; set; } = string.Empty;
}