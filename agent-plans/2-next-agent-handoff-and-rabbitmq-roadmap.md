# Next Agent Plan: UI integration + RabbitMQ roadmap

This document is a handoff plan for the next agent. It captures **current repo state**, **what is already done**, and a **concrete next execution path**:

- **Angular → HousingApi integration (Steps 3–5) is already implemented**.
- **Angular → TodoApi integration is now implemented** (`/todos` page, `TodoService`).
- **Angular → HousingApi applications list is now implemented** (`/applications` page, `getAllApplications()`).
- **Navbar** refactored: single topbar with active-link highlighting (`RouterLinkActive`).
- Next focus: the **RabbitMQ event-driven microservices** work (HousingApi/BookStoreApi producers, TodoApi consumer + reverse “todo completed” events).
- See “Dev ports” table below — BookStoreApi is `https://localhost:7153` (was not previously documented).

---

## Repo structure (relevant parts)

- `client/` (Angular UI)
- `HousingApi/` (Mongo-backed ASP.NET Core REST API)
- `TodoApi/` (ASP.NET Core REST API + SQL Server EF Core)
- `BookStoreApi/` (Mongo-backed bookstore sample API; future producer of “book created” events)
- `Microservices.sln` (root solution that includes service projects)

---

## Dev ports (verified from launchSettings.json)

| Service      | HTTPS                   | HTTP                   | Notes                        |
|--------------|-------------------------|------------------------|------------------------------|
| HousingApi   | `https://localhost:7152` | `http://localhost:5125` | Mongo-backed                 |
| BookStoreApi | `https://localhost:7153` | `http://localhost:5126` | Mongo-backed                 |
| TodoApi      | `https://localhost:7236` | `http://localhost:5188` | SQL Server / EF Core-backed  |
| Angular UI   | `http://localhost:4200`  | —                      | `ng serve`                   |

---

## What is already implemented (done)

### HousingApi: Locations CRUD (Mongo)

- Endpoints:
  - `GET /api/locations`
  - `GET /api/locations/{id:length(24)}`
  - `POST /api/locations`
  - `PUT /api/locations/{id:length(24)}`
  - `DELETE /api/locations/{id:length(24)}`

- Mongo config key: `HousingDatabase` in `HousingApi/appsettings.json`
  - `DatabaseName`: `Housing`
  - `LocationsCollectionName`: `Locations`
  - `ApplicationsCollectionName`: `Applications`
  - `ConnectionString`: `mongodb://admin:password123@localhost:27018/?authSource=admin`

- Notes:
  - JSON uses default ASP.NET camelCase (so Angular can consume without mapping).
  - IDs are Mongo `ObjectId` strings (24 chars).
  - CORS: `HousingApi/Program.cs` allows Angular dev origins for both `4200` and `5173` (http+https).

### HousingApi: Applications collection + endpoints (Mongo)

- Goal: store submissions from the Angular “apply” form
  - `firstName`, `lastName`, `email`, `createdAt`, `housingId`

- Endpoints:
  - `GET /api/applications` (list all)
  - `GET /api/applications/{id:length(24)}`
  - `POST /api/applications`

- Behavior:
  - `housingId` validated as ObjectId and checked for existence in Locations.
  - `createdAt` set server-side (`DateTime.UtcNow`).

### Naming convention (applied in HousingApi)

HousingApi uses explicit naming:

- Entities: `*Entity`
- Requests: `*Request`
- Responses: `*Response`

Important: ensure class names and file names stay aligned (C# best practice).

Current types of interest:
- Locations entity: `HousingApi.Models.HousingLocationEntity`
- Applications entity: `HousingApi.Models.HousingApplicationEntity`
- Applications request: `HousingApi.Models.CreateHousingApplicationRequest`
- Applications response: `HousingApi.Models.HousingApplicationResponse`

### Solution naming cleanup

Inside `HousingApi/`, renamed project artifacts away from `BookStoreApi.*`:

- `HousingApi/HousingApi.csproj`
- `HousingApi/HousingApi.sln`
- `HousingApi/HousingApi.http`

Root solution `Microservices.sln` exists for monorepo IntelliSense.

---

## Known local dev/tooling gotchas

### C# IntelliSense in Cursor/VS Code

If “Go to definition” doesn’t work:
- Choose **one** solution to load (prefer `Microservices.sln` for monorepo).
- If there are multiple `.sln` prompts, “Choose and save”.
- Ensure the service builds; C# language server becomes noisy when the project doesn’t compile.

### Build errors after renames

If errors mention missing types like `HousingApplicationDto`, it usually means the code references weren’t updated after a rename. Prefer “Rename Symbol” (F2) over renaming files manually.

---

## Angular UI integration status (completed)

### What was done (original)
- Replaced JSON-server reads with real API calls.
- Switched Angular route id and model id to `string` (Mongo ObjectId).
- Added submit flow to POST applications to the API.

### What was done (latest session)

#### New pages
- **`/todos`** — `client/src/app/todos/todos.ts`
  - Displays all todos from TodoApi (`GET https://localhost:7236/api/TodoItems`).
  - Shows id, name, and completion status; completed items are visually struck through.
- **`/applications`** — `client/src/app/applications/applications.ts`
  - Displays all housing applications from HousingApi (`GET https://localhost:7152/api/applications`).
  - Table layout with id, name, email, housingId, and formatted submission date.

#### New service method
- `HousingService.getAllApplications()` added to `client/src/app/housing.service.ts`.
  - Returns `Observable<HousingApplicationInfo[]>`; reuses the existing `urlApplications` field.
  - No new service file created — kept all HousingApi methods in one service.

#### New service file
- `client/src/app/todo.service.ts` — `TodoService` with `getAllTodos()`.
  - Separate service (not HousingService) because it hits a different backend (TodoApi).

#### New model file
- `client/src/app/todo.ts` — `TodoItem` interface matching `TodoItemDTO` from TodoApi.

#### Architecture pattern used (consistent across both pages)
- Service method returns `Observable<T[]>` (lazy — HTTP fires only on subscribe).
- Component assigns that Observable to a `$`-suffixed field, no `.subscribe()` called.
- Template uses `| async` pipe — subscribes on render, unsubscribes on destroy automatically.
- `catchError` in each service method emits `[]` on failure so UI never crashes.
- Constructor injection style (matching user's preference over `inject()`).

#### Navbar
- `app.ts` / `app.css` updated: logo + brand name + nav links in one `<header class="topbar">` flex row.
- Added `RouterLinkActive` — the current route's link gets an `active` CSS class (filled purple pill).
- Nav links: Home, Todos, Applications.

### Current Angular→API URL map
> **Updated in 5.1** — Angular services now use relative paths; the Angular dev-server proxy resolves the backend. See `agent-plans/5.1-angular-dev-proxy-setup.md`.

| Angular call             | Backend     | Proxy target (local)        | Proxy target (compose)      |
|--------------------------|-------------|-----------------------------|-----------------------------|
| `/api/locations`         | HousingApi  | `http://localhost:5125`     | `http://localhost:7152`     |
| `/api/applications`      | HousingApi  | `http://localhost:5125`     | `http://localhost:7152`     |
| `/api/TodoItems`         | TodoApi     | `http://localhost:5188`     | `http://localhost:5188`     |

### Verification checklist (still worth re-running locally)
- UI loads list from API (no JSON server running).
- UI details page loads by ObjectId string.
- Submitting the form creates a document in Mongo `Applications` collection.
- `GET /api/applications` shows it.
- `/todos` page loads all SQL todo rows from TodoApi.
- `/applications` page loads all Mongo application documents from HousingApi.

---

### Local run commands
> **Updated in 5.1/5.2** — use the proxy-aware npm scripts; prefer the `http` launch profile to avoid SSL/proxy issues.

- UI (local APIs): `npm run start:local` (from `client/`)
- UI (compose APIs): `npm run start:compose` (from `client/`)
- HousingApi: `dotnet watch --launch-profile http` (from `HousingApi/`)
- TodoApi: `dotnet watch --launch-profile http` (from `TodoApi/`)

See `agent-plans/5.2-dev-startup-modes.md` for the full breakdown of local vs compose startup modes.

## RabbitMQ roadmap (event-driven microservices)

This is not required for Step 3/4, but should guide the next phase.

### Event flows

1. **HousingApplicationCreated**
   - Producer: HousingApi (after application persisted)
   - Consumer: TodoApi
   - Effect: create a new SQL todo row “Review housing application …”

2. **BookCreated**
   - Producer: BookStoreApi (after book persisted)
   - Consumer: TodoApi
   - Effect: create a new SQL todo row “Process new book …”

3. **TodoCompleted** (created from an event)
   - Producer: TodoApi (when todo transitions to completed)
   - Consumer: the originating service (HousingApi or BookStoreApi)
   - Effect: increment “completed counter” in the respective database.
     - Housing: likely Mongo (or later SQL if introduced).
     - Books: Mongo (or SQL if introduced later).

### Implementation approach (recommended)

- Use Docker Compose to run:
  - RabbitMQ
  - MongoDB
  - SQL Server
  - services
  - Angular UI (optional)

- Use a message bus library for .NET (pick one):
  - **MassTransit** (commonly used with RabbitMQ)
  - or raw `RabbitMQ.Client` for learning simplicity

- Use explicit message contracts (shared package or duplicated contracts with versioning discipline).

### Reliability note (future)

When publishing events after DB writes, consider an outbox pattern (especially in SQL/TodoApi) if you need stronger guarantees. For interview prep, be ready to explain this trade-off.

---

## Tech stack targets (per user)

- Angular 18 (later; currently the repo client may not be pinned to 18 yet)
  - NGXS, RxJS, custom CSS
- ASP.NET Core 7 (later; current services are currently newer in repo; plan migration as a separate step)
- Docker, Microservices
- RabbitMQ (required)
- HTTPS, Socket (maybe later)
- Mongo, SQL (required)
- Kafka (optional later)
- Keycloak / IDP (optional later)

