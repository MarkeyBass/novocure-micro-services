cmd shift p ->
`>.NET: Restart Language Server`

terminal comand to start the api server locally
`dotnet run --launch-profile https`

## EF - ORM
EF = Entity Framework Core
- the .NET ORM used here to talk to SQL Server
- migrations are versioned database schema changes

Database - run migrations (from `TodoApi/`):
`dotnet ef database update`

Migration process:
- existing migration files describe the database schema
- `dotnet ef database update` creates the database if needed and applies pending migrations
- after that, tables like `TodoItems` should exist

If dotnet ef is missing
`dotnet tool install --global dotnet-ef`
