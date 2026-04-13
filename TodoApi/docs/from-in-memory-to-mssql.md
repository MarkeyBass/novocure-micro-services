# From in-memory database to SQL Server (MSSQL)

This page describes the path this project took to switch Entity Framework Core from **`UseInMemoryDatabase`** to **SQL Server in Docker**, including configuration, common errors, and schema creation.

Verification against your working tree (no commit yet) was done with:

```bash
git diff
git status -sb
```

---

## End-to-end path

```mermaid
flowchart LR
  A[InMemory + TodoContext] --> B[NuGet: SqlServer provider]
  B --> C[Docker: MSSQL on localhost:1433]
  C --> D[Connection string + TrustServerCertificate]
  D --> E[Create database TodoApi]
  E --> F[EF migrations + database update]
  F --> G[App queries TodoItems table]
```

1. **Keep `TodoContext` as-is** — `DbContext` + `DbSet<TodoItem>` work for both providers; no change required there for a basic switch.
2. **Reference the SQL Server provider** — `Microsoft.EntityFrameworkCore.SqlServer` (same major version as the rest of EF, e.g. 9.0.x for `net9.0`). Design-time packages (`Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`) support `dotnet ef` migrations.
3. **Run SQL Server** — e.g. `databases/docker-compose.yaml` service `mssql`: port **1433**, `sa` password from **`MSSQL_SA_PASSWORD`**.
4. **Register SQL Server in DI** — replace `UseInMemoryDatabase` with `UseSqlServer(connectionString)`.
5. **Put secrets and server settings in configuration** — `ConnectionStrings:TodoContext` in `appsettings.Development.json` (or user secrets / env vars for real secrets).
6. **Create the database** — e.g. `CREATE DATABASE TodoApi;` in a SQL client, or let `dotnet ef database update` create it when applying migrations (typical).
7. **Create schema** — `dotnet ef migrations add InitialCreate` then `dotnet ef database update` so the **`TodoItems`** table exists (fixes `Invalid object name 'TodoItems'`).

---

## What changed in this repo (from `git diff`)

| Area | Change |
|------|--------|
| `Program.cs` | Added `using Microsoft.EntityFrameworkCore.SqlServer;`. Commented `UseInMemoryDatabase("TodoList")`. Registered `AddDbContext<TodoContext>` with `UseSqlServer(builder.Configuration.GetConnectionString("TodoContext"))`. |
| `appsettings.Development.json` | Added `ConnectionStrings.TodoContext` pointing at `localhost,1433`, database `TodoApi`, `sa` + password aligned with Docker, and **`TrustServerCertificate=True`**. |
| `notes.txt` | Jotted `dotnet ef migrations add` / `dotnet ef database update` reminders. |

**Untracked (new) from `git status`:** `Migrations/` — e.g. `20260412014430_InitialCreate.cs`, `InitialCreate.Designer.cs`, `TodoContextModelSnapshot.cs`. Commit these with the rest when you are ready.

`TodoApi.csproj` was not in the current diff (already contained `Microsoft.EntityFrameworkCore.SqlServer`, InMemory, Tools, Design, etc. on the committed baseline).

---

## Configuration reference

### Docker (`databases/docker-compose.yaml`)

- **Image:** `mcr.microsoft.com/mssql/server:2022-latest`
- **Port:** `1433:1433` → connect from the host as **`localhost,1433`**
- **`sa` password:** `MSSQL_SA_PASSWORD` (example in compose: `YourStrongPassword123!`)

### Connection string (development)

Example shape (match password and DB name to your environment):

```text
Server=localhost,1433;Database=TodoApi;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;
```

- **`TrustServerCertificate=True`** — common for local Docker SQL Server when the server certificate is not in your machine trust store (avoids TLS pre-login / certificate rejection during handshake).

### `Program.cs` pattern

```csharp
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext")));
```

---

## Problems you may hit (and what they mean)

| Symptom | Likely cause | Direction |
|--------|----------------|-----------|
| `AuthenticationException` / certificate rejected during pre-login | Encrypted connection + untrusted server cert | Add **`TrustServerCertificate=True`** (dev only) or proper trust setup |
| `Cannot open database "TodoApi"` / login failed | Database **does not exist** yet (or wrong password) | `CREATE DATABASE TodoApi;` and verify `sa` password matches compose |
| `Invalid object name 'TodoItems'` | DB exists but **no tables** | Run **`dotnet ef migrations add`** / **`dotnet ef database update`** |
| EF suggests `EnableRetryOnFailure` | Wrapper around a SQL error | Fix the **underlying** SQL error; retries do not replace missing DB/tables |

---

## Commands checklist

```bash
# From repo root / database folder — start SQL Server
docker compose -f databases/docker-compose.yaml up -d mssql

# From project folder (TodoApi.csproj)
dotnet ef migrations add InitialCreate
dotnet ef database update

dotnet run --launch-profile https
```

If `dotnet ef` is missing:

```bash
dotnet tool install --global dotnet-ef
# Use a version compatible with your EF Core major version (e.g. 9.x for EF 9).
```

---

## Optional: switching back to in-memory

1. In `Program.cs`, use `UseInMemoryDatabase("TodoList")` again instead of `UseSqlServer(...)`.
2. Remove or ignore the SQL connection string for local runs.

You can keep both packages (`InMemory` + `SqlServer`) and flip registration while learning.

---

## Security note

Do not commit production passwords. For learning, `appsettings.Development.json` is acceptable locally; prefer **User Secrets** or environment variables for anything shared or long-lived.
