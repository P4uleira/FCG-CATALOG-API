using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Commands.CompletePurchase;

public sealed class CompletePurchaseCommandHandler : IRequestHandler<CompletePurchaseCommand, CompletePurchaseResult>
{
    private const string ApprovedStatus = "Approved";

    private readonly IUserLibraryRepository _userLibraryRepository;

    private readonly IPurchaseHistoryRepository _purchaseHistoryRepository;

    private readonly ILogger<CompletePurchaseCommandHandler> _logger;

    public CompletePurchaseCommandHandler(
        IUserLibraryRepository userLibraryRepository,
        IPurchaseHistoryRepository purchaseHistoryRepository,
        ILogger<CompletePurchaseCommandHandler> logger)
    {
        _userLibraryRepository = userLibraryRepository;
        _purchaseHistoryRepository = purchaseHistoryRepository;
        _logger = logger;
    }

    public async Task<CompletePurchaseResult> Handle(CompletePurchaseCommand request,CancellationToken cancellationToken)
    {
        if (!string.Equals(request.Status, ApprovedStatus, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Pagamento rejeitado. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, Status: {Status}",
                request.OrderId,
                request.UserId,
                request.GameId,
                request.Status);

            await RegistrarHistoricoAsync(request, cancellationToken);

            return new CompletePurchaseResult(
                request.OrderId,
                request.UserId,
                request.GameId,
                CompletePurchaseStatus.PaymentRejected);
        }

        _logger.LogInformation(
            "Pagamento aprovado. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",request.OrderId,request.UserId,request.GameId);

        var purchaseAlreadyExists =
            await _userLibraryRepository.ExistsAsync(
                request.UserId,
                request.GameId,
                cancellationToken);

        if (purchaseAlreadyExists)
        {
            _logger.LogInformation(
                "Compra já existente. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                request.OrderId,
                request.UserId,
                request.GameId);

            await RegistrarHistoricoAsync(request, cancellationToken);

            return new CompletePurchaseResult(
                request.OrderId,
                request.UserId,
                request.GameId,
                CompletePurchaseStatus.AlreadyExists);
        }

        var userLibrary = UserLibrary.Create(
            request.UserId,
            request.GameId,
            request.ProcessedAt);

        await _userLibraryRepository.AddAsync(
            userLibrary,
            cancellationToken);

        _logger.LogInformation(
            "Jogo adicionado à biblioteca. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, UserLibraryId: {UserLibraryId}",
            request.OrderId,
            request.UserId,
            request.GameId,
            userLibrary.Id);

        await RegistrarHistoricoAsync(request, cancellationToken);

        return new CompletePurchaseResult(
            request.OrderId,
            request.UserId,
            request.GameId,
            CompletePurchaseStatus.Added);
    }

    private async Task RegistrarHistoricoAsync(
    CompletePurchaseCommand request,
    CancellationToken cancellationToken)
    {
        try
        {
            var purchaseHistoryEntry = PurchaseHistoryEntry.Create(
                request.OrderId,
                request.UserId,
                request.GameId,
                request.Price,
                request.Status,
                request.ProcessedAt);

            await _purchaseHistoryRepository.AddAsync( purchaseHistoryEntry,cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Falha ao registrar histórico de compra no MongoDB. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                request.OrderId,
                request.UserId,
                request.GameId);
        }
    }
}