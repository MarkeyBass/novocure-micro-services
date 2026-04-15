using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HousingApi.Models;

public class HousingLocationEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Photo { get; set; } = null!;

    public int AvailableUnits { get; set; }

    public bool Wifi { get; set; }

    public bool Laundry { get; set; }
}
