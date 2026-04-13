using HousingApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HousingApi.Services;

public class HousingLocationsService
{
    private readonly IMongoCollection<HousingLocation> _locationsCollection;

    public HousingLocationsService(IOptions<HousingDatabaseSettings> housingDatabaseSettings)
    {
        var mongoClient = new MongoClient(housingDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(housingDatabaseSettings.Value.DatabaseName);

        _locationsCollection = mongoDatabase.GetCollection<HousingLocation>(
            housingDatabaseSettings.Value.LocationsCollectionName
        );
    }

    public async Task<List<HousingLocation>> GetAsync() =>
        await _locationsCollection.Find(_ => true).ToListAsync();

    public async Task<HousingLocation?> GetAsync(string id) =>
        await _locationsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(HousingLocation newLocation) =>
        await _locationsCollection.InsertOneAsync(newLocation);

    public async Task UpdateAsync(string id, HousingLocation updatedLocation) =>
        await _locationsCollection.ReplaceOneAsync(x => x.Id == id, updatedLocation);

    public async Task RemoveAsync(string id) =>
        await _locationsCollection.DeleteOneAsync(x => x.Id == id);
}
