# Infrastructure - Database & Migrations

Database schema management and infrastructure scripts

## Structure

```
infra/
└── db/
    ├── migrations/        # EF Core migration files (auto-generated)
    ├── scripts/           # Manual SQL scripts (if needed)
    └── README.md          # This file
```

## Database: SQL Server

### Local Development Options

#### Option 1: Docker Container (Recommended)
```powershell
# Run SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourPassword123!" `
  -p 1433:1433 --name sqlserver-local `
  -d mcr.microsoft.com/mssql/server:2022-latest

# Stop container
docker stop sqlserver-local

# Start existing container
docker start sqlserver-local
```

#### Option 2: SQL Server Express (Local Installation)
Download and install SQL Server Express from Microsoft:
https://www.microsoft.com/sql-server/sql-server-downloads

### Connection String

**appsettings.Development.json** (backend):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TournamentPlatformDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True"
  }
}
```

## EF Core Migrations

### Create Migration
```powershell
cd backend/src/Project.Infrastructure

# Add new migration
dotnet ef migrations add InitialCreate --startup-project ../Project.Api

# View SQL that will be executed
dotnet ef migrations script --startup-project ../Project.Api
```

### Apply Migrations
```powershell
# Update database to latest migration
dotnet ef database update --startup-project ../Project.Api

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --startup-project ../Project.Api
```

### Remove Last Migration
```powershell
# Remove migration (only if not applied to database)
dotnet ef migrations remove --startup-project ../Project.Api
```

## Database Schema

### Core Entities
- **Users** - Base user accounts (Organizers and Players)
- **Organizers** - Tournament organizers (1:1 with Users)
- **Players** - Tournament players (1:1 with Users)
- **Teams** - Player groups for tournament enrollment
- **TeamInvitations** - Pending team invitations
- **Tournaments** - Tournament definitions
- **Matches** - Tournament matches
- **Standings** - Calculated tournament standings
- **ScoringSystem** - Scoring configuration (enum or strategy)

### Key Relationships
- User → Organizer (1:1)
- User → Player (1:1)
- Organizer → Tournaments (1:N)
- Player → Teams (N:M via TeamMembers)
- Tournament → Teams (N:M via Enrollments)
- Tournament → Matches (1:N)
- Match → Teams (2:N for participants)

## Seeding Test Data

### Development Seed Data
Create in `Project.Infrastructure/Data/DbSeeder.cs`:
- Sample organizer accounts
- Sample player accounts
- Sample tournaments (draft, active, completed)
- Sample teams and enrollments

### Seed on Startup
```csharp
// In Program.cs
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}
```

## Manual Scripts

For operations not handled by EF Core migrations:
- Data fixes
- Performance optimizations (indexes)
- Stored procedures (if needed)

Place SQL files in `infra/db/scripts/` with descriptive names:
- `001-add-tournament-indexes.sql`
- `002-fix-orphaned-teams.sql`

## Database Maintenance

### Backup (Development)
```powershell
# Backup database
docker exec sqlserver-local /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "YourPassword123!" `
  -Q "BACKUP DATABASE TournamentPlatformDb TO DISK='/var/opt/mssql/backup/tournament.bak'"

# Copy backup from container
docker cp sqlserver-local:/var/opt/mssql/backup/tournament.bak ./tournament.bak
```

### Restore (Development)
```powershell
# Copy backup to container
docker cp ./tournament.bak sqlserver-local:/var/opt/mssql/backup/

# Restore database
docker exec sqlserver-local /opt/mssql-tools/bin/sqlcmd `
  -S localhost -U sa -P "YourPassword123!" `
  -Q "RESTORE DATABASE TournamentPlatformDb FROM DISK='/var/opt/mssql/backup/tournament.bak' WITH REPLACE"
```

## Troubleshooting

### Connection Refused
- Verify SQL Server container is running: `docker ps`
- Check port 1433 is not blocked by firewall
- Verify connection string matches container configuration

### Migration Errors
- Ensure Project.Api is set as startup project
- Check EF Core tools installed: `dotnet tool install --global dotnet-ef`
- Verify connection string in appsettings.Development.json

### Database Locked
- Close all active connections (SQL Server Management Studio, Azure Data Studio)
- Restart SQL Server container: `docker restart sqlserver-local`
