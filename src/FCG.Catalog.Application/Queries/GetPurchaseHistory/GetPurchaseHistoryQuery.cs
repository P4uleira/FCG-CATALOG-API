using FCG.Catalog.Application.DTOs;
using MediatR;

namespace FCG.Catalog.Application.Queries.GetPurchaseHistory;

public sealed record GetPurchaseHistoryQuery(Guid UserId)
    : IRequest<IReadOnlyList<PurchaseHistoryDto>>;