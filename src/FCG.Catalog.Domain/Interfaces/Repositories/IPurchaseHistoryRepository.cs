using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Domain.Interfaces.Repositories;

public interface IPurchaseHistoryRepository
{
    Task AddAsync(
        PurchaseHistoryEntry entry,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PurchaseHistoryEntry>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}