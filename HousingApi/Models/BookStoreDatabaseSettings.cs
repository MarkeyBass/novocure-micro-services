namespace BookStoreApi.Models;

public class BookStoreDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string BooksCollectionName { get; set; } = null!;
}


// The preceding BookStoreDatabaseSettings class is used to store the appsettings.json file's BookStoreDatabase property values. The JSON and C# property names are named identically to ease the mapping process.
