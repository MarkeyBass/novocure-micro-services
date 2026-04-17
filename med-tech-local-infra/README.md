# Local Infrastructure

```
cd med-tech-local-infra
docker compose up -d
docker compose down        # stop
docker compose down -v     # stop + wipe all data
```

---

## MongoDB

**Browser** → http://localhost:8081 (admin / admin123)

**Terminal**
```bash
docker exec -it mongo mongosh -u admin -p password123 --authenticationDatabase admin
```
```js
show dbs
use Housing
db.Locations.find().pretty()
```

---

## SQL Server

**Browser** → http://localhost:8978
First run: complete the setup wizard, then add a connection:
- Host: `sqlserver` (if connecting from another container) or `localhost`
- Port: `1433`
- User: `sa` / Password: `YourStrongPassword123!`

**Terminal**
```bash
docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrongPassword123!" -No
```
```sql
SELECT name FROM sys.databases;
GO
USE TodoApi;
SELECT * FROM TodoItems;
GO
```

---

## Seed data

Seed the locations into Mongo via HousingApi (run after `docker compose up -d`):

```bash
for row in $(cat seed-locations.json | python3 -c "import sys,json; [print(json.dumps(r)) for r in json.load(sys.stdin)]"); do
  curl -s -o /dev/null -w "%{http_code}\n" -X POST http://localhost:7152/api/locations \
    -H "Content-Type: application/json" -d "$row"
done
```

IDs are assigned by MongoDB — `id` is not included in `seed-locations.json`.

---

## HousingApi

**Browser** → http://localhost:7152/swagger (Swagger UI, Development only)

**Terminal** (smoke test)
```bash
curl http://localhost:7152/api/locations
curl http://localhost:7152/api/applications
```

### Development workflow

Run on the host for active development — same reasoning as TodoApi:

```bash
cd HousingApi
dotnet watch run --launch-profile https
```

The API connects to the containerised Mongo via `localhost:27018`. Use the containerised `housing-api` service for full-stack or integration testing.

---

## TodoApi

**Browser** → http://localhost:5188/swagger (Swagger UI, Development only)

**Terminal** (smoke test)
```bash
curl http://localhost:5188/api/TodoItems
```

> **Note:** EF Core migrations run automatically on startup via `Database.Migrate()` — the `TodoApi` database and schema are created on first boot.

### Development workflow

**Don't use Docker for TodoApi during active development.** Run it on the host instead:

```bash
cd TodoApi
dotnet watch run --launch-profile https
```

The API connects to the containerised SQL Server via `localhost:1433` (the port is exposed to the host). You get instant hot reload and easy debugger attachment.

Use the containerised `todo-api` service only when testing the full stack together (e.g. RabbitMQ wiring, integration tests).

> **Why not volume-mount + `dotnet watch` inside Docker?**
> Docker Desktop on macOS polls for file changes rather than using native OS events — hot reload is slow and unreliable. The SDK image is also ~900 MB vs ~220 MB for the runtime image. The host is the better dev environment for .NET.

---

## RabbitMQ

**Browser** → http://localhost:15672 (admin / admin)

**Terminal** (list queues)
```bash
docker exec -it rabbitmq rabbitmqctl list_queues name messages
```

---

## Ports at a glance

| Service       | Purpose          | Port  |
|---------------|------------------|-------|
| MongoDB       | TCP              | 27018 |
| mongo-express | Browser UI       | 8081  |
| SQL Server    | TCP              | 1433  |
| CloudBeaver   | Browser UI       | 8978  |
| HousingApi    | HTTP             | 7152  |
| TodoApi       | HTTP             | 5188  |
| RabbitMQ      | AMQP             | 5672  |
| RabbitMQ      | Management UI    | 15672 |

## Named volumes

- `mongo_data`
- `sqlserver_data`
- `rabbitmq_data`



## Test docker-compose
```bash
cd med-tech-local-infra
docker compose up -d
docker compose ps
```

## Then verify in the browser:
```bash
http://localhost:8081
http://localhost:8978
http://localhost:15672
```