using FCG.Catalog.Application.DTOs;
using MediatR;

namespace FCG.Catalog.Application.Commands.PurchaseGame;

public sealed record PurchaseGameCommand(
    Guid GameId,
    Guid UserId) : IRequest<PurchaseGameResponse>;