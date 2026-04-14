using HousingApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HousingApi.Services;

public class HousingApplicationsService
{
    private readonly IMongoCollection<HousingApplication> _applicationsCollection;

    public HousingApplicationsService(IOptions<HousingDatabaseSettings> housingDatabaseSettings)
    {
        var mongoClient = new MongoClient(housingDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(housingDatabaseSettings.Value.DatabaseName);

        _applicationsCollection = mongoDatabase.GetCollection<HousingApplication>(
            housingDatabaseSettings.Value.ApplicationsCollectionName
        );
    }

    public async Task<List<HousingApplication>> GetAsync() =>
        await _applicationsCollection.Find(_ => true).ToListAsync();

    public async Task<HousingApplication?> GetAsync(string id) =>
        await _applicationsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(HousingApplication newApplication) =>
        await _applicationsCollection.InsertOneAsync(newApplication);
}
