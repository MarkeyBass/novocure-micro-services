using HousingApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HousingApi.Services;

public class HousingApplicationsService
{
    private readonly IMongoCollection<HousingApplicationEntity> _applicationsCollection;

    public HousingApplicationsService(IOptions<HousingDatabaseSettings> housingDatabaseSettings)
    {
        var mongoClient = new MongoClient(housingDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(housingDatabaseSettings.Value.DatabaseName);

        _applicationsCollection = mongoDatabase.GetCollection<HousingApplicationEntity>(
            housingDatabaseSettings.Value.ApplicationsCollectionName
        );
    }

    public async Task<List<HousingApplicationEntity>> GetAsync() =>
        await _applicationsCollection.Find(_ => true).ToListAsync();

    public async Task<HousingApplicationEntity?> GetAsync(string id) =>
        await _applicationsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(HousingApplicationEntity newApplication) =>
        await _applicationsCollection.InsertOneAsync(newApplication);

    // Atomically increments CompletedReviewCount and sets LastReviewedAt using MongoDB update operators.
    // Using $inc + $set rather than FindAsync/ReplaceOneAsync avoids a read-modify-write race condition.
    public async Task IncrementCompletedReviewAsync(string id, DateTime reviewedAt)
    {
        var update = Builders<HousingApplicationEntity>
            .Update.Inc(x => x.CompletedReviewCount, 1)
            .Set(x => x.LastReviewedAt, reviewedAt);

        await _applicationsCollection.UpdateOneAsync(x => x.Id == id, update);
    }
}
