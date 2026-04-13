# How `BookStoreDatabase` settings find their way into code

ASP.NET Core reads **JSON** from `appsettings.json` (and environment-specific variants like `appsettings.Development.json`). That file is just data; your app turns it into a real object when something asks for it.

## What binds to what

The `BookStoreDatabase` object in JSON lines up with the `BookStoreDatabaseSettings` class in `Models/`. **Property names match key names**—`ConnectionString` in JSON maps to `ConnectionString` on the class, and so on. The configuration binder does not guess from abbreviations or synonyms; it pairs names so the mapping stays obvious in code reviews and diffs.

```json
"BookStoreDatabase": {
  "ConnectionString": "...",
  "DatabaseName": "...",
  "BooksCollectionName": "..."
}
```

```csharp
public class BookStoreDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string BooksCollectionName { get; set; } = null!;
}
```

## What `Configure<BookStoreDatabaseSettings>(...)` actually does

This line does **not** inject anything by itself:

```csharp
builder.Services.Configure<BookStoreDatabaseSettings>(
    builder.Configuration.GetSection("BookStoreDatabase"));
```

It **registers a recipe** in the DI container: “when code needs `IOptions<BookStoreDatabaseSettings>`, create a `BookStoreDatabaseSettings` instance and **fill its properties** from the `BookStoreDatabase` section.” No custom constructor is required—the binder sets public properties after constructing the object.

So: **JSON supplies values, names connect them to C# properties, and `Configure` wires that binding into the options system** so services can receive a strongly typed, reload-aware settings object instead of raw strings scattered through the codebase.
