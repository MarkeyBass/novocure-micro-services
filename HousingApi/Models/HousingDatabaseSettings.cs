namespace HousingApi.Models;

public class HousingDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string LocationsCollectionName { get; set; } = null!;

    public string ApplicationsCollectionName { get; set; } = null!;
}
