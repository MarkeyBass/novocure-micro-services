# Local Infrastructure

```
cd dbs-docker-compose
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
| RabbitMQ      | AMQP             | 5672  |
| RabbitMQ      | Management UI    | 15672 |
