using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Commands.CompletePurchase;

public sealed class CompletePurchaseCommandHandler
	: IRequestHandler<
		CompletePurchaseCommand,
		CompletePurchaseResult>
{
	private const string ApprovedStatus = "Approved";

	private readonly IUserLibraryRepository _userLibraryRepository;

	private readonly ILogger<CompletePurchaseCommandHandler> _logger;

	public CompletePurchaseCommandHandler(
		IUserLibraryRepository userLibraryRepository,
		ILogger<CompletePurchaseCommandHandler> logger)
	{
		_userLibraryRepository = userLibraryRepository;
		_logger = logger;
	}

	public async Task<CompletePurchaseResult> Handle(
		CompletePurchaseCommand request,
		CancellationToken cancellationToken)
	{
		if (!string.Equals(
				request.Status,
				ApprovedStatus,
				StringComparison.OrdinalIgnoreCase))
		{
			_logger.LogWarning(
				"Pagamento rejeitado. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, Status: {Status}",
				request.OrderId,
				request.UserId,
				request.GameId,
				request.Status);

			return new CompletePurchaseResult(
				request.OrderId,
				request.UserId,
				request.GameId,
				CompletePurchaseStatus.PaymentRejected);
		}

		_logger.LogInformation(
			"Pagamento aprovado. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
			request.OrderId,
			request.UserId,
			request.GameId);

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

		return new CompletePurchaseResult(
			request.OrderId,
			request.UserId,
			request.GameId,
			CompletePurchaseStatus.Added);
	}
}