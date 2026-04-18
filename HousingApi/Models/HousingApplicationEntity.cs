using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HousingApi.Models;

public class HousingApplicationEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string HousingId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    // Incremented each time a linked TodoCompleted event is received.
    // Existing documents without this field deserialize to 0 (C# int default) — no migration needed.
    public int CompletedReviewCount { get; set; }

    // Set to the CompletedAt timestamp of the most recent review. Null until first review.
    public DateTime? LastReviewedAt { get; set; }
}
