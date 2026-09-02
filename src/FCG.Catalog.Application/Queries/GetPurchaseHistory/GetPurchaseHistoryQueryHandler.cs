using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Queries.GetPurchaseHistory;

public sealed class GetPurchaseHistoryQueryHandler
    : IRequestHandler<GetPurchaseHistoryQuery, IReadOnlyList<PurchaseHistoryDto>>
{
    private readonly IPurchaseHistoryRepository _purchaseHistoryRepository;

    private readonly ILogger<GetPurchaseHistoryQueryHandler> _logger;

    public GetPurchaseHistoryQueryHandler(
        IPurchaseHistoryRepository purchaseHistoryRepository,
        ILogger<GetPurchaseHistoryQueryHandler> logger)
    {
        _purchaseHistoryRepository = purchaseHistoryRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PurchaseHistoryDto>> Handle(
        GetPurchaseHistoryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Consultando historico de compras. UserId: {UserId}",
            request.UserId);

        var entries = await _purchaseHistoryRepository.GetByUserIdAsync(
            request.UserId,
            cancellationToken);

        return entries
            .Select(entry => new PurchaseHistoryDto(
                entry.OrderId,
                entry.GameId,
                entry.Price,
                entry.Status,
                entry.ProcessedAt,
                entry.RegisteredAt))
            .ToList();
    }
}