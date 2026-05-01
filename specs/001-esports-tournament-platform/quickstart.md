# Quickstart: Esports Tournament Management Platform

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.x |
| Node.js | 20+ |
| npm | 10+ |
| Docker Desktop | Any recent version |
| Git | Any |

---

## 1. Clone the Repository

```bash
git clone <repo-url>
cd esports-230548-250602-318798/esportsApp
```

---

## 2. Configure Environment Variables

Copy the example environment file for the backend:

```bash
cp backend/EsportsApp.API/.env.example backend/EsportsApp.API/.env
```

Edit `backend/EsportsApp.API/.env` and set the following values:

```env
CONNECTION_STRING=Server=localhost,1433;Database=EsportsDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
JWT_SECRET=<minimum-32-character-random-string>
JWT_ISSUER=EsportsApp
JWT_AUDIENCE=EsportsApp
ADMIN_EMAIL=admin@esports.local
ADMIN_PASSWORD=Admin@1234
```

> **Security note**: Never commit `.env` files. The `.gitignore` must exclude them. All credentials must come from environment variables — never hardcoded.

---

## 3. Start SQL Server via Docker

```bash
docker run \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  --name esports-sql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Wait ~10 seconds for SQL Server to initialize before running migrations.

---

## 4. Apply Database Migrations

From the `backend/` directory:

```bash
cd backend
dotnet ef database update --project EsportsApp.Infrastructure --startup-project EsportsApp.API
```

This command:
- Creates the `EsportsDb` database.
- Applies all EF Core migrations.
- Seeds the Videogame catalog (League of Legends, Valorant, CS2, Dota 2, Rocket League).
- Creates the default Admin account using `ADMIN_EMAIL` and `ADMIN_PASSWORD` from the environment.

---

## 5. Start the Backend API

```bash
cd backend
dotnet run --project EsportsApp.API
```

| Endpoint | URL |
|----------|-----|
| API (HTTPS) | `https://localhost:5001` |
| API (HTTP) | `http://localhost:5000` |
| Swagger UI | `https://localhost:5001/swagger` |
| SignalR Hub | `https://localhost:5001/hubs/tournament` |

---

## 6. Start the Angular Frontend

In a separate terminal:

```bash
cd frontend
npm install
npm start
```

Angular dev server: `http://localhost:4200`

---

## 7. Run Unit Tests

```bash
cd backend
dotnet test EsportsApp.Tests
```

Test output will report coverage for TR-001 through TR-004.

---

## Docker Compose (All-in-One)

Alternatively, bring up all services with one command from the project root:

```bash
docker-compose up --build
```

This starts:
1. SQL Server (port 1433)
2. Backend API (port 5001)
3. Angular frontend (port 4200)

Stop all services:

```bash
docker-compose down
```

---

## Seeded Data After First Migration

| Data | Details |
|------|---------|
| Videogames | League of Legends, Valorant, CS2, Dota 2, Rocket League |
| Admin account | Email: `ADMIN_EMAIL` env var; Password: `ADMIN_PASSWORD` env var |

---

## Default Service Ports

| Service | Port |
|---------|------|
| Angular dev server | 4200 |
| ASP.NET Core API (HTTPS) | 5001 |
| ASP.NET Core API (HTTP) | 5000 |
| SQL Server | 1433 |
| SignalR Hub path | `/hubs/tournament` (same host as API) |

---

## Verifying the Setup

1. Open `https://localhost:5001/swagger` — Swagger UI should load with all API endpoints listed.
2. Open `http://localhost:4200` — Angular app home page should load.
3. Register a Player account via the UI or Swagger.
4. Log in and verify a JWT token is returned.
5. Connect to the SignalR hub at `https://localhost:5001/hubs/tournament` to verify the WebSocket handshake.
