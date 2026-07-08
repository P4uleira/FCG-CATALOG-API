using MediatR;

namespace FCG.Catalog.Application.Commands.DeleteGame;

public class DeleteGameCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}