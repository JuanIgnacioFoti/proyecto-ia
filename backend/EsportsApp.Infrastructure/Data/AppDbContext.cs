using Microsoft.EntityFrameworkCore;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;

namespace EsportsApp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<Videogame> Videogames => Set<Videogame>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamInvitation> TeamInvitations => Set<TeamInvitation>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<ScoringSystem> ScoringSystems => Set<ScoringSystem>();
    public DbSet<TournamentRegistration> TournamentRegistrations => Set<TournamentRegistration>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Standing> Standings => Set<Standing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasConversion<string>().IsRequired();
        });

        // Player (1:1 with User)
        modelBuilder.Entity<Player>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.User)
                .WithOne(u => u.Player)
                .HasForeignKey<Player>(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => p.Username).IsUnique();
            e.Property(p => p.Username).IsRequired().HasMaxLength(50);
            e.Property(p => p.RealName).IsRequired().HasMaxLength(100);
            e.HasOne(p => p.MainVideogame)
                .WithMany(v => v.Players)
                .HasForeignKey(p => p.MainVideogameId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Organizer (1:1 with User)
        modelBuilder.Entity<Organizer>(e =>
        {
            e.HasKey(o => o.Id);
            e.HasOne(o => o.User)
                .WithOne(u => u.Organizer)
                .HasForeignKey<Organizer>(o => o.Id)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(o => o.OrganizationName).IsUnique();
            e.Property(o => o.OrganizationName).IsRequired().HasMaxLength(60);
        });

        // Videogame
        modelBuilder.Entity<Videogame>(e =>
        {
            e.HasKey(v => v.Id);
            e.HasIndex(v => v.Name).IsUnique();
            e.Property(v => v.Name).IsRequired().HasMaxLength(100);
        });

        // Team
        modelBuilder.Entity<Team>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => new { t.Name, t.VideogameId }).IsUnique();
            e.Property(t => t.Name).IsRequired().HasMaxLength(100);
            e.HasOne(t => t.Videogame)
                .WithMany(v => v.Teams)
                .HasForeignKey(t => t.VideogameId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Captain)
                .WithMany()
                .HasForeignKey(t => t.CaptainId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TeamMember
        modelBuilder.Entity<TeamMember>(e =>
        {
            e.HasKey(tm => tm.Id);
            e.HasIndex(tm => new { tm.TeamId, tm.PlayerId }).IsUnique();
            e.HasOne(tm => tm.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(tm => tm.Player)
                .WithMany(p => p.TeamMemberships)
                .HasForeignKey(tm => tm.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TeamInvitation
        modelBuilder.Entity<TeamInvitation>(e =>
        {
            e.HasKey(ti => ti.Id);
            e.Property(ti => ti.Status).HasConversion<string>().IsRequired();
            e.HasOne(ti => ti.Team)
                .WithMany(t => t.Invitations)
                .HasForeignKey(ti => ti.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ti => ti.InvitedPlayer)
                .WithMany(p => p.ReceivedInvitations)
                .HasForeignKey(ti => ti.InvitedPlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Ignore(ti => ti.IsExpired);
            e.Ignore(ti => ti.EffectiveStatus);
        });

        // Tournament
        modelBuilder.Entity<Tournament>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(200);
            e.Property(t => t.Status).HasConversion<string>().IsRequired();
            e.Property(t => t.PreSuspensionStatus).HasConversion<string>();
            e.HasOne(t => t.Videogame)
                .WithMany(v => v.Tournaments)
                .HasForeignKey(t => t.VideogameId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Organizer)
                .WithMany(o => o.Tournaments)
                .HasForeignKey(t => t.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ScoringSystem
        modelBuilder.Entity<ScoringSystem>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.TournamentId).IsUnique();
            e.Property(s => s.Type).HasConversion<string>().IsRequired();
            e.HasOne(s => s.Tournament)
                .WithOne(t => t.ScoringSystem)
                .HasForeignKey<ScoringSystem>(s => s.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TournamentRegistration
        modelBuilder.Entity<TournamentRegistration>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => new { r.TournamentId, r.TeamId }).IsUnique();
            e.Property(r => r.Status).HasConversion<string>().IsRequired();
            e.HasOne(r => r.Tournament)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Team)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Match
        modelBuilder.Entity<Match>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);
            e.ToTable(tb => tb.HasCheckConstraint("CK_Match_HomeAwayDiff", "\"HomeTeamId\" <> \"AwayTeamId\""));
        });

        // Standing
        modelBuilder.Entity<Standing>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.TournamentId, s.TeamId }).IsUnique();
            e.Property(s => s.RowVersion).IsRowVersion();
            e.HasOne(s => s.Tournament)
                .WithMany(t => t.Standings)
                .HasForeignKey(s => s.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Team)
                .WithMany()
                .HasForeignKey(s => s.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
