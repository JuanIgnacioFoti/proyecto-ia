using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Data;

public static class SeedDataInitializer
{
    private static readonly Guid LeagueOfLegendsId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ValorantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid CS2Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Dota2Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid RocketLeagueId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // --- Demo player user IDs ---
    private static readonly Guid P01UserId = Guid.Parse("a0000001-0000-0000-0000-000000000001");
    private static readonly Guid P02UserId = Guid.Parse("a0000001-0000-0000-0000-000000000002");
    private static readonly Guid P03UserId = Guid.Parse("a0000001-0000-0000-0000-000000000003");
    private static readonly Guid P04UserId = Guid.Parse("a0000001-0000-0000-0000-000000000004");
    private static readonly Guid P05UserId = Guid.Parse("a0000001-0000-0000-0000-000000000005");
    private static readonly Guid P06UserId = Guid.Parse("a0000001-0000-0000-0000-000000000006");
    private static readonly Guid P07UserId = Guid.Parse("a0000001-0000-0000-0000-000000000007");
    private static readonly Guid P08UserId = Guid.Parse("a0000001-0000-0000-0000-000000000008");
    private static readonly Guid P09UserId = Guid.Parse("a0000001-0000-0000-0000-000000000009");
    private static readonly Guid P10UserId = Guid.Parse("a0000001-0000-0000-0000-000000000010");
    private static readonly Guid P11UserId = Guid.Parse("a0000001-0000-0000-0000-000000000011");
    private static readonly Guid P12UserId = Guid.Parse("a0000001-0000-0000-0000-000000000012");
    private static readonly Guid P13UserId = Guid.Parse("a0000001-0000-0000-0000-000000000013");
    private static readonly Guid P14UserId = Guid.Parse("a0000001-0000-0000-0000-000000000014");
    private static readonly Guid P15UserId = Guid.Parse("a0000001-0000-0000-0000-000000000015");

    // --- Demo organizer user IDs ---
    private static readonly Guid Org1UserId = Guid.Parse("b0000001-0000-0000-0000-000000000001");
    private static readonly Guid Org2UserId = Guid.Parse("b0000001-0000-0000-0000-000000000002");

    // --- Team IDs ---
    private static readonly Guid TeamDragonSlayersId = Guid.Parse("c0000001-0000-0000-0000-000000000001");
    private static readonly Guid TeamVoidWalkersId   = Guid.Parse("c0000001-0000-0000-0000-000000000002");
    private static readonly Guid TeamIronPhoenixId   = Guid.Parse("c0000001-0000-0000-0000-000000000003");
    private static readonly Guid TeamSolarStormId    = Guid.Parse("c0000001-0000-0000-0000-000000000004");
    private static readonly Guid TeamNeonWolvesId    = Guid.Parse("c0000001-0000-0000-0000-000000000005");
    private static readonly Guid TeamCyberGuardsId   = Guid.Parse("c0000001-0000-0000-0000-000000000006");
    private static readonly Guid TeamPhantomEdgeId   = Guid.Parse("c0000001-0000-0000-0000-000000000007");

    // --- Tournament IDs ---
    private static readonly Guid LolChampionshipId  = Guid.Parse("d0000001-0000-0000-0000-000000000001");
    private static readonly Guid ValorantCupId      = Guid.Parse("d0000001-0000-0000-0000-000000000002");

    public static async Task SeedAsync(AppDbContext context, string adminEmail, string adminPassword)
    {
        // Seed videogames
        var gameSeeds = new[]
        {
            new Videogame { Id = LeagueOfLegendsId, Name = "League of Legends" },
            new Videogame { Id = ValorantId,        Name = "Valorant" },
            new Videogame { Id = CS2Id,             Name = "CS2" },
            new Videogame { Id = Dota2Id,           Name = "Dota 2" },
            new Videogame { Id = RocketLeagueId,    Name = "Rocket League" },
        };

        foreach (var game in gameSeeds)
        {
            if (!await context.Videogames.AnyAsync(v => v.Id == game.Id))
                context.Videogames.Add(game);
        }

        await context.SaveChangesAsync();

        // Seed admin user
        if (!await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            });
            await context.SaveChangesAsync();
        }

        // Skip demo data if already seeded
        if (await context.Players.AnyAsync(p => p.Id == P01UserId))
            return;

        // -------------------------------------------------------
        // Demo organizers
        // -------------------------------------------------------
        var demoPassword = BCrypt.Net.BCrypt.HashPassword("Demo1234!");

        var org1User = new User { Id = Org1UserId, Email = "org1@demo.com", PasswordHash = demoPassword, Role = UserRole.Organizer, CreatedAt = DateTime.UtcNow, IsActive = true };
        var org2User = new User { Id = Org2UserId, Email = "org2@demo.com", PasswordHash = demoPassword, Role = UserRole.Organizer, CreatedAt = DateTime.UtcNow, IsActive = true };
        context.Users.AddRange(org1User, org2User);

        var org1 = new Organizer { Id = Org1UserId, OrganizationName = "Alpha Esports League" };
        var org2 = new Organizer { Id = Org2UserId, OrganizationName = "Beta Gaming Circuit" };
        context.Organizers.AddRange(org1, org2);

        // -------------------------------------------------------
        // Demo players (15 players)
        // -------------------------------------------------------
        var playerData = new[]
        {
            (Id: P01UserId, Email: "player1@demo.com",  Username: "NightWolf",   RealName: "Carlos Mendez",    Game: LeagueOfLegendsId),
            (Id: P02UserId, Email: "player2@demo.com",  Username: "FrostByte",   RealName: "Ana García",       Game: LeagueOfLegendsId),
            (Id: P03UserId, Email: "player3@demo.com",  Username: "ShadowArrow", RealName: "Miguel Torres",    Game: LeagueOfLegendsId),
            (Id: P04UserId, Email: "player4@demo.com",  Username: "VoidHunter",  RealName: "Laura Pérez",      Game: LeagueOfLegendsId),
            (Id: P05UserId, Email: "player5@demo.com",  Username: "StormKing",   RealName: "Diego Ramírez",    Game: LeagueOfLegendsId),
            (Id: P06UserId, Email: "player6@demo.com",  Username: "CrimsonEdge", RealName: "Sofía Ruiz",       Game: LeagueOfLegendsId),
            (Id: P07UserId, Email: "player7@demo.com",  Username: "IronBlast",   RealName: "Javier López",     Game: LeagueOfLegendsId),
            (Id: P08UserId, Email: "player8@demo.com",  Username: "PhoenixRise", RealName: "Isabella Mora",    Game: LeagueOfLegendsId),
            (Id: P09UserId, Email: "player9@demo.com",  Username: "CyberWitch",  RealName: "Andrés Vega",      Game: LeagueOfLegendsId),
            (Id: P10UserId, Email: "player10@demo.com", Username: "BlazeFist",   RealName: "Valentina Cruz",   Game: LeagueOfLegendsId),
            (Id: P11UserId, Email: "player11@demo.com", Username: "NeonBlade",   RealName: "Sebastián Díaz",   Game: LeagueOfLegendsId),
            (Id: P12UserId, Email: "player12@demo.com", Username: "DarkArrow",   RealName: "Camila Flores",    Game: LeagueOfLegendsId),
            (Id: P13UserId, Email: "player13@demo.com", Username: "ShadowFox",   RealName: "Tomás Herrera",    Game: ValorantId),
            (Id: P14UserId, Email: "player14@demo.com", Username: "CyberViper",  RealName: "Martina Salazar",  Game: ValorantId),
            (Id: P15UserId, Email: "player15@demo.com", Username: "GhostRecon",  RealName: "Nicolás Castillo", Game: ValorantId),
        };

        foreach (var (id, email, username, realName, game) in playerData)
        {
            context.Users.Add(new User   { Id = id, Email = email, PasswordHash = demoPassword, Role = UserRole.Player, CreatedAt = DateTime.UtcNow, IsActive = true });
            context.Players.Add(new Player { Id = id, Username = username, RealName = realName, MainVideogameId = game });
        }

        await context.SaveChangesAsync();

        // -------------------------------------------------------
        // Teams
        // -------------------------------------------------------
        // LoL teams (captain = first member)
        var teamDragon = new Team { Id = TeamDragonSlayersId, Name = "Dragon Slayers", VideogameId = LeagueOfLegendsId, CaptainId = P01UserId, CreatedAt = DateTime.UtcNow };
        var teamVoid   = new Team { Id = TeamVoidWalkersId,   Name = "Void Walkers",   VideogameId = LeagueOfLegendsId, CaptainId = P04UserId, CreatedAt = DateTime.UtcNow };
        var teamIron   = new Team { Id = TeamIronPhoenixId,   Name = "Iron Phoenix",   VideogameId = LeagueOfLegendsId, CaptainId = P07UserId, CreatedAt = DateTime.UtcNow };
        var teamSolar  = new Team { Id = TeamSolarStormId,    Name = "Solar Storm",    VideogameId = LeagueOfLegendsId, CaptainId = P10UserId, CreatedAt = DateTime.UtcNow };

        // Valorant teams
        var teamNeon    = new Team { Id = TeamNeonWolvesId,  Name = "Neon Wolves",    VideogameId = ValorantId, CaptainId = P13UserId, CreatedAt = DateTime.UtcNow };
        var teamCyber   = new Team { Id = TeamCyberGuardsId, Name = "Cyber Guards",   VideogameId = ValorantId, CaptainId = P14UserId, CreatedAt = DateTime.UtcNow };
        var teamPhantom = new Team { Id = TeamPhantomEdgeId, Name = "Phantom Edge",   VideogameId = ValorantId, CaptainId = P15UserId, CreatedAt = DateTime.UtcNow };

        context.Teams.AddRange(teamDragon, teamVoid, teamIron, teamSolar, teamNeon, teamCyber, teamPhantom);
        await context.SaveChangesAsync();

        // Team members
        var members = new[]
        {
            // Dragon Slayers: P01, P02, P03
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamDragonSlayersId, PlayerId = P01UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamDragonSlayersId, PlayerId = P02UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamDragonSlayersId, PlayerId = P03UserId, JoinedAt = DateTime.UtcNow },
            // Void Walkers: P04, P05, P06
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamVoidWalkersId, PlayerId = P04UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamVoidWalkersId, PlayerId = P05UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamVoidWalkersId, PlayerId = P06UserId, JoinedAt = DateTime.UtcNow },
            // Iron Phoenix: P07, P08, P09
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamIronPhoenixId, PlayerId = P07UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamIronPhoenixId, PlayerId = P08UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamIronPhoenixId, PlayerId = P09UserId, JoinedAt = DateTime.UtcNow },
            // Solar Storm: P10, P11, P12
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamSolarStormId, PlayerId = P10UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamSolarStormId, PlayerId = P11UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamSolarStormId, PlayerId = P12UserId, JoinedAt = DateTime.UtcNow },
            // Neon Wolves: P13 (only captain — Valorant cup still open for more)
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamNeonWolvesId, PlayerId = P13UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamCyberGuardsId, PlayerId = P14UserId, JoinedAt = DateTime.UtcNow },
            new TeamMember { Id = Guid.NewGuid(), TeamId = TeamPhantomEdgeId, PlayerId = P15UserId, JoinedAt = DateTime.UtcNow },
        };
        context.TeamMembers.AddRange(members);
        await context.SaveChangesAsync();

        // -------------------------------------------------------
        // Tournament 1 – LoL Spring Championship (Completed)
        // -------------------------------------------------------
        var lolChampionship = new Tournament
        {
            Id = LolChampionshipId,
            Name = "LoL Spring Championship 2026",
            VideogameId = LeagueOfLegendsId,
            OrganizerId = Org1UserId,
            Status = TournamentStatus.Completed,
            MaxTeams = 8,
            MinMembersPerTeam = 3,
            StartDate = DateTime.UtcNow.AddMonths(-2),
            EstimatedEndDate = DateTime.UtcNow.AddMonths(-1),
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
        };
        context.Tournaments.Add(lolChampionship);

        var lolScoring = new ScoringSystem
        {
            Id = Guid.NewGuid(),
            TournamentId = LolChampionshipId,
            Type = ScoringSystemType.Standard,
            WinPoints = 3,
            DrawPoints = 1,
            LossPoints = 0,
        };
        context.ScoringSystems.Add(lolScoring);

        // Registrations for LoL championship
        var lolRegistrations = new[]
        {
            new TournamentRegistration { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, TeamId = TeamDragonSlayersId, Status = RegistrationStatus.Active, RegisteredAt = DateTime.UtcNow.AddMonths(-3) },
            new TournamentRegistration { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, TeamId = TeamVoidWalkersId,   Status = RegistrationStatus.Active, RegisteredAt = DateTime.UtcNow.AddMonths(-3) },
            new TournamentRegistration { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, TeamId = TeamIronPhoenixId,   Status = RegistrationStatus.Active, RegisteredAt = DateTime.UtcNow.AddMonths(-3) },
            new TournamentRegistration { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, TeamId = TeamSolarStormId,    Status = RegistrationStatus.Active, RegisteredAt = DateTime.UtcNow.AddMonths(-3) },
        };
        context.TournamentRegistrations.AddRange(lolRegistrations);

        await context.SaveChangesAsync();

        // Round-robin matches (6 matches total):
        // DS 2-0 VW  → DS wins
        // DS 1-1 IP  → draw
        // DS 3-1 SS  → DS wins
        // VW 2-1 IP  → VW wins
        // VW 0-2 SS  → SS wins
        // IP 1-1 SS  → draw
        var baseDate = DateTime.UtcNow.AddMonths(-2);
        var lolMatches = new[]
        {
            new Match { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, HomeTeamId = TeamDragonSlayersId, AwayTeamId = TeamVoidWalkersId,   HomeScore = 2, AwayScore = 0, PlayedAt = baseDate.AddDays(0),  RecordedAt = baseDate.AddDays(0) },
            new Match { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, HomeTeamId = TeamDragonSlayersId, AwayTeamId = TeamIronPhoenixId,   HomeScore = 1, AwayScore = 1, PlayedAt = baseDate.AddDays(3),  RecordedAt = baseDate.AddDays(3) },
            new Match { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, HomeTeamId = TeamDragonSlayersId, AwayTeamId = TeamSolarStormId,    HomeScore = 3, AwayScore = 1, PlayedAt = baseDate.AddDays(6),  RecordedAt = baseDate.AddDays(6) },
            new Match { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, HomeTeamId = TeamVoidWalkersId,   AwayTeamId = TeamIronPhoenixId,   HomeScore = 2, AwayScore = 1, PlayedAt = baseDate.AddDays(9),  RecordedAt = baseDate.AddDays(9) },
            new Match { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, HomeTeamId = TeamVoidWalkersId,   AwayTeamId = TeamSolarStormId,    HomeScore = 0, AwayScore = 2, PlayedAt = baseDate.AddDays(12), RecordedAt = baseDate.AddDays(12) },
            new Match { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, HomeTeamId = TeamIronPhoenixId,   AwayTeamId = TeamSolarStormId,    HomeScore = 1, AwayScore = 1, PlayedAt = baseDate.AddDays(15), RecordedAt = baseDate.AddDays(15) },
        };
        context.Matches.AddRange(lolMatches);

        // Final standings (Win=3, Draw=1, Loss=0):
        //  1. Dragon Slayers  3G 2W 1D 0L  7pts
        //  2. Solar Storm     3G 1W 1D 1L  4pts
        //  3. Void Walkers    3G 1W 0D 2L  3pts
        //  4. Iron Phoenix    3G 0W 1D 2L  1pt
        var lolStandings = new[]
        {
            new Standing { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, TeamId = TeamDragonSlayersId, Points = 7, MatchesPlayed = 3, Wins = 2, Draws = 1, Losses = 0 },
            new Standing { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, TeamId = TeamSolarStormId,    Points = 4, MatchesPlayed = 3, Wins = 1, Draws = 1, Losses = 1 },
            new Standing { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, TeamId = TeamVoidWalkersId,   Points = 3, MatchesPlayed = 3, Wins = 1, Draws = 0, Losses = 2 },
            new Standing { Id = Guid.NewGuid(), TournamentId = LolChampionshipId, TeamId = TeamIronPhoenixId,   Points = 1, MatchesPlayed = 3, Wins = 0, Draws = 1, Losses = 2 },
        };
        context.Standings.AddRange(lolStandings);

        // -------------------------------------------------------
        // Tournament 2 – Valorant Cup 2026 (Open / registrations)
        // -------------------------------------------------------
        var valorantCup = new Tournament
        {
            Id = ValorantCupId,
            Name = "Valorant Cup 2026",
            VideogameId = ValorantId,
            OrganizerId = Org2UserId,
            Status = TournamentStatus.Open,
            MaxTeams = 8,
            MinMembersPerTeam = 1,
            StartDate = DateTime.UtcNow.AddMonths(1),
            EstimatedEndDate = DateTime.UtcNow.AddMonths(2),
            CreatedAt = DateTime.UtcNow.AddDays(-7),
        };
        context.Tournaments.Add(valorantCup);

        var valScoring = new ScoringSystem
        {
            Id = Guid.NewGuid(),
            TournamentId = ValorantCupId,
            Type = ScoringSystemType.WinnerTakesAll,
            WinPoints = 3,
            DrawPoints = 1,
            LossPoints = 0,
        };
        context.ScoringSystems.Add(valScoring);

        var valorantRegistrations = new[]
        {
            new TournamentRegistration { Id = Guid.NewGuid(), TournamentId = ValorantCupId, TeamId = TeamNeonWolvesId,  Status = RegistrationStatus.Active, RegisteredAt = DateTime.UtcNow.AddDays(-5) },
            new TournamentRegistration { Id = Guid.NewGuid(), TournamentId = ValorantCupId, TeamId = TeamCyberGuardsId, Status = RegistrationStatus.Active, RegisteredAt = DateTime.UtcNow.AddDays(-4) },
            new TournamentRegistration { Id = Guid.NewGuid(), TournamentId = ValorantCupId, TeamId = TeamPhantomEdgeId, Status = RegistrationStatus.Active, RegisteredAt = DateTime.UtcNow.AddDays(-3) },
        };
        context.TournamentRegistrations.AddRange(valorantRegistrations);

        await context.SaveChangesAsync();
    }
}
