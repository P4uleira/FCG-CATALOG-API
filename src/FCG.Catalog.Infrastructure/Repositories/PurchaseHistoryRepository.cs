using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces.Repositories;
using FCG.Catalog.Infrastructure.Data.Documents;
using MongoDB.Driver;

namespace FCG.Catalog.Infrastructure.Repositories;

public class PurchaseHistoryRepository : IPurchaseHistoryRepository
{
    private const string CollectionName = "purchase_history";

    private readonly IMongoCollection<PurchaseHistoryDocument> _collection;

    public PurchaseHistoryRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PurchaseHistoryDocument>(CollectionName);
    }

    public async Task AddAsync(
        PurchaseHistoryEntry entry,
        CancellationToken cancellationToken)
    {
        var document = ToDocument(entry);

        await _collection.InsertOneAsync(
            document,
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseHistoryEntry>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var filter = Builders<PurchaseHistoryDocument>.Filter
            .Eq(document => document.UserId, userId);

        var documents = await _collection
            .Find(filter)
            .SortByDescending(document => document.ProcessedAt)
            .ToListAsync(cancellationToken);

        return documents
            .Select(ToEntity)
            .ToList();
    }

    private static PurchaseHistoryDocument ToDocument(PurchaseHistoryEntry entry) =>
        new()
        {
            Id = entry.Id,
            OrderId = entry.OrderId,
            UserId = entry.UserId,
            GameId = entry.GameId,
            Price = entry.Price,
            Status = entry.Status,
            ProcessedAt = entry.ProcessedAt,
            RegisteredAt = entry.RegisteredAt
        };

    private static PurchaseHistoryEntry ToEntity(PurchaseHistoryDocument document) =>
        PurchaseHistoryEntry.Reconstruct(
            document.Id,
            document.OrderId,
            document.UserId,
            document.GameId,
            document.Price,
            document.Status,
            document.ProcessedAt,
            document.RegisteredAt);
}