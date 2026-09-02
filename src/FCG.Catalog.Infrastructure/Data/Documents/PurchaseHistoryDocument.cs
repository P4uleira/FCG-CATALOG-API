using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FCG.Catalog.Infrastructure.Data.Documents;

public class PurchaseHistoryDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid OrderId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid GameId { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; }

    public DateTime RegisteredAt { get; set; }
}