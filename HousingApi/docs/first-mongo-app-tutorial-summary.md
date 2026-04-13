# First MongoDB web API tutorial — summary

**Official walkthrough:** [Create a web API with ASP.NET Core and MongoDB](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-9.0&tabs=visual-studio-code) (.NET 9, Visual Studio Code tab)

This doc is a **short companion**: it captures the story and order of steps, not every code block. Use Microsoft Learn for copy-paste accuracy and updates.

---

## What you build

A minimal **BookStore** REST API: **CRUD** on a `Books` collection in MongoDB, plus tweaks so JSON looks the way you expect.

---

## Prerequisites (tutorial)

- MongoDB and **mongosh** on your machine, **.NET 9 SDK**, VS Code + **C# Dev Kit** (for the VS Code path).

---

## 1. MongoDB side

- Run **`mongod`** with a chosen `--dbpath`.
- In **`mongosh`**: `use BookStore`, `createCollection('Books')`, **`insertMany`** two sample books (fields like `Name`, `Price`, `Category`, `Author`).
- Documents get Mongo’s **`_id`** (`ObjectId`); the API maps that to a string `Id` in C#.

*(You can use Docker instead; then your connection string and auth must match that setup—Learn assumes local Mongo on the default port.)*

---

## 2. ASP.NET Core project

- **`dotnet new webapi -o BookStoreApi --use-controllers`**
- **`dotnet add package MongoDB.Driver`**

---

## 3. `Book` model

- **`[BsonId]`** + **`[BsonRepresentation(BsonType.ObjectId)]`** on `Id` (string in C#, `ObjectId` in Mongo).
- **`[BsonElement("Name")]`** on the C# property that maps to the BSON field **`Name`** (tutorial uses `BookName` in C#).

---

## 4. Configuration

- **`appsettings.json`**: `BookStoreDatabase` with `ConnectionString`, `DatabaseName`, `BooksCollectionName`.
- **`BookStoreDatabaseSettings`** POCO with matching property names.
- **`Program.cs`**: `Configure<BookStoreDatabaseSettings>(GetSection("BookStoreDatabase"))` so **`IOptions<BookStoreDatabaseSettings>`** gets a bound instance (no custom constructor on the settings class).

---

## 5. `BooksService`

- Injects **`IOptions<BookStoreDatabaseSettings>`**, builds **`MongoClient`** → database → **`IMongoCollection<Book>`**.
- CRUD: **`Find`**, **`InsertOneAsync`**, **`ReplaceOneAsync`**, **`DeleteOneAsync`** (async variants).
- Register as **`AddSingleton<BooksService>`** because **`MongoClient`** should be reused (driver guidance).

---

## 6. `BooksController`

- Route **`api/[controller]`**; id routes use **`{id:length(24)}`** (ObjectId string length).
- **POST** returns **`CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook)`** → **201** plus **`Location`** pointing at the **GET-by-id** URL (not `Post`).

---

## 7. JSON shape (System.Text.Json)

- **`AddJsonOptions`** → **`PropertyNamingPolicy = null`** so API JSON uses **PascalCase** like your C# types (e.g. `Author` not `author`).
- **`[JsonPropertyName("Name")]`** on `BookName` so the HTTP JSON property is **`Name`** while BSON still uses **`Name`** via **`[BsonElement]`**.

---

## 8. Test the API (VS Code path in Learn)

- **`dotnet add package NSwag.AspNetCore`**
- In Development: **`MapOpenApi()`** + **`UseSwaggerUi`** with **`DocumentPath = "/openapi/v1.json"`**
- Run the app, open **Swagger UI** (`/swagger`), exercise **GET / POST / GET by id / DELETE** (and PUT in the full tutorial).

Visual Studio’s path uses **`.http` files** and Endpoints Explorer instead—same API, different tooling.

---

## Mental model

| Layer | Role |
|--------|------|
| `appsettings.json` | Connection + database + collection names |
| `IOptions<>` | Typed settings from configuration |
| `BooksService` | One `MongoClient` / collection; all data access |
| Controller | HTTP verbs, status codes, **`Location`** on create |

When something in your repo diverges from Learn (e.g. auth, port **27018**, Docker env vars), fix the **connection string** and **deployment** story—the tutorial’s **code structure** still applies.
