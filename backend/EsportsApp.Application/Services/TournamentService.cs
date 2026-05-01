using EsportsApp.Application.DTOs.Tournaments;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;

namespace EsportsApp.Application.Services;

public class TournamentService : ITournamentService
{
    private readonly ITournamentRepository _tournaments;
    private readonly IVideogameRepository _videogames;
    private readonly ITournamentRegistrationRepository _registrations;

    public TournamentService(ITournamentRepository tournaments, IVideogameRepository videogames,
        ITournamentRegistrationRepository registrations)
    {
        _tournaments = tournaments;
        _videogames = videogames;
        _registrations = registrations;
    }

    public async Task<TournamentDetailDto> GetByIdAsync(Guid id, Guid? requestingUserId)
    {
        var t = await _tournaments.GetByIdAsync(id)
            ?? throw new NotFoundException("Tournament not found.");

        if (t.Status == TournamentStatus.Draft && t.OrganizerId != requestingUserId)
            throw new NotFoundException("Tournament not found.");

        var registeredCount = await _registrations.GetActiveCountByTournamentAsync(id);
        return MapToDetail(t, registeredCount);
    }

    public async Task<IEnumerable<TournamentSummaryDto>> GetPublicListAsync()
    {
        var list = await _tournaments.GetPublicListAsync();
        var result = new List<TournamentSummaryDto>();
        foreach (var t in list)
        {
            var count = await _registrations.GetActiveCountByTournamentAsync(t.Id);
            result.Add(MapToSummary(t, count));
        }
        return result;
    }

    public async Task<IEnumerable<TournamentSummaryDto>> GetMyTournamentsAsync(Guid organizerId)
    {
        var list = await _tournaments.GetByOrganizerAsync(organizerId);
        var result = new List<TournamentSummaryDto>();
        foreach (var t in list)
        {
            var count = await _registrations.GetActiveCountByTournamentAsync(t.Id);
            result.Add(MapToSummary(t, count));
        }
        return result;
    }

    public async Task<TournamentDetailDto> CreateAsync(Guid organizerId, CreateTournamentRequestDto request)
    {
        if (!await _videogames.ExistsAsync(request.VideogameId))
            throw new ValidationException("videogameId", "Videogame not found.");

        if (request.StartDate <= DateTime.UtcNow)
            throw new ValidationException("startDate", "Start date must be in the future.");

        if (request.EstimatedEndDate <= request.StartDate)
            throw new ValidationException("estimatedEndDate", "Estimated end date must be after start date.");

        var scoring = BuildScoringSystem(request.ScoringSystem);

        var tournament = new Tournament
        {
            OrganizerId = organizerId,
            Name = request.Name,
            Description = request.Description,
            VideogameId = request.VideogameId,
            StartDate = request.StartDate,
            EstimatedEndDate = request.EstimatedEndDate,
            MaxTeams = request.MaxTeams,
            MinMembersPerTeam = request.MinMembersPerTeam,
            ScoringSystem = scoring,
        };

        await _tournaments.AddAsync(tournament);
        await _tournaments.SaveChangesAsync();

        return await GetByIdAsync(tournament.Id, organizerId);
    }

    public async Task<TournamentDetailDto> UpdateAsync(Guid organizerId, Guid tournamentId, UpdateTournamentRequestDto request)
    {
        var t = await _tournaments.GetByIdAsync(tournamentId)
            ?? throw new NotFoundException("Tournament not found.");

        if (t.OrganizerId != organizerId)
            throw new ForbiddenException("Not the tournament owner.");

        if (t.Status == TournamentStatus.Completed)
            throw new ForbiddenException("Cannot modify a completed tournament.");

        if (request.Name != null) t.Name = request.Name;
        if (request.Description != null) t.Description = request.Description;
        if (request.StartDate.HasValue)
        {
            if (request.StartDate.Value <= DateTime.UtcNow)
                throw new ValidationException("startDate", "Start date must be in the future.");
            t.StartDate = request.StartDate.Value;
        }
        if (request.EstimatedEndDate.HasValue) t.EstimatedEndDate = request.EstimatedEndDate.Value;
        if (request.MaxTeams.HasValue) t.MaxTeams = request.MaxTeams.Value;
        if (request.MinMembersPerTeam.HasValue) t.MinMembersPerTeam = request.MinMembersPerTeam.Value;

        await _tournaments.UpdateAsync(t);
        await _tournaments.SaveChangesAsync();

        var count = await _registrations.GetActiveCountByTournamentAsync(t.Id);
        return MapToDetail(t, count);
    }

    public async Task AdvanceStatusAsync(Guid organizerId, Guid tournamentId)
    {
        var t = await _tournaments.GetByIdAsync(tournamentId)
            ?? throw new NotFoundException("Tournament not found.");

        if (t.OrganizerId != organizerId)
            throw new ForbiddenException("Not the tournament owner.");

        t.Status = t.Status switch
        {
            TournamentStatus.Draft => TournamentStatus.Open,
            TournamentStatus.Open => TournamentStatus.InProgress,
            TournamentStatus.InProgress => TournamentStatus.Completed,
            _ => throw new UnprocessableEntityException($"Cannot advance from status {t.Status}."),
        };

        await _tournaments.UpdateAsync(t);
        await _tournaments.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid organizerId, Guid tournamentId)
    {
        var t = await _tournaments.GetByIdAsync(tournamentId)
            ?? throw new NotFoundException("Tournament not found.");

        if (t.OrganizerId != organizerId)
            throw new ForbiddenException("Not the tournament owner.");

        if (t.Status == TournamentStatus.InProgress || t.Status == TournamentStatus.Completed)
            throw new ForbiddenException("Cannot delete an in-progress or completed tournament.");

        await _tournaments.DeleteAsync(t);
        await _tournaments.SaveChangesAsync();
    }

    public async Task SuspendAsync(Guid adminId, Guid tournamentId)
    {
        var t = await _tournaments.GetByIdAsync(tournamentId)
            ?? throw new NotFoundException("Tournament not found.");

        if (t.Status == TournamentStatus.Completed)
            throw new UnprocessableEntityException("Cannot suspend a completed tournament.");

        if (t.Status == TournamentStatus.Suspended)
            throw new ConflictException("Tournament is already suspended.");

        t.PreSuspensionStatus = t.Status;
        t.Status = TournamentStatus.Suspended;

        await _tournaments.UpdateAsync(t);
        await _tournaments.SaveChangesAsync();
    }

    public async Task ReinstateAsync(Guid adminId, Guid tournamentId)
    {
        var t = await _tournaments.GetByIdAsync(tournamentId)
            ?? throw new NotFoundException("Tournament not found.");

        if (t.Status != TournamentStatus.Suspended)
            throw new UnprocessableEntityException("Tournament is not suspended.");

        t.Status = t.PreSuspensionStatus ?? TournamentStatus.Draft;
        t.PreSuspensionStatus = null;

        await _tournaments.UpdateAsync(t);
        await _tournaments.SaveChangesAsync();
    }

    private static ScoringSystem BuildScoringSystem(ScoringSystemRequestDto dto)
    {
        if (!Enum.TryParse<ScoringSystemType>(dto.Type, true, out var type))
            throw new ValidationException("scoringSystem.type", $"Invalid scoring type: {dto.Type}");

        return type switch
        {
            ScoringSystemType.Standard => new ScoringSystem { Type = ScoringSystemType.Standard, WinPoints = 3, DrawPoints = 1, LossPoints = 0 },
            ScoringSystemType.WinnerTakesAll => new ScoringSystem { Type = ScoringSystemType.WinnerTakesAll, WinPoints = 3, DrawPoints = 0, LossPoints = 0 },
            ScoringSystemType.Custom => BuildCustomScoring(dto),
            _ => throw new ValidationException("scoringSystem.type", "Invalid scoring type."),
        };
    }

    private static ScoringSystem BuildCustomScoring(ScoringSystemRequestDto dto)
    {
        if (!dto.WinPoints.HasValue || !dto.DrawPoints.HasValue || !dto.LossPoints.HasValue)
            throw new ValidationException("scoringSystem", "Custom scoring requires winPoints, drawPoints, lossPoints.");

        if (dto.LossPoints.Value < 0 || dto.DrawPoints.Value < dto.LossPoints.Value || dto.WinPoints.Value < dto.DrawPoints.Value)
            throw new ValidationException("scoringSystem", "Must satisfy: winPoints >= drawPoints >= lossPoints >= 0.");

        return new ScoringSystem
        {
            Type = ScoringSystemType.Custom,
            WinPoints = dto.WinPoints.Value,
            DrawPoints = dto.DrawPoints.Value,
            LossPoints = dto.LossPoints.Value,
        };
    }

    private static TournamentSummaryDto MapToSummary(Tournament t, int registeredCount) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Status = t.Status.ToString(),
        VideogameId = t.VideogameId,
        VideogameName = t.Videogame?.Name ?? string.Empty,
        OrganizerName = t.Organizer?.OrganizationName ?? string.Empty,
        StartDate = t.StartDate,
        EstimatedEndDate = t.EstimatedEndDate,
        MaxTeams = t.MaxTeams,
        RegisteredTeams = registeredCount,
    };

    private static TournamentDetailDto MapToDetail(Tournament t, int registeredCount) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        Status = t.Status.ToString(),
        VideogameId = t.VideogameId,
        VideogameName = t.Videogame?.Name ?? string.Empty,
        OrganizerName = t.Organizer?.OrganizationName ?? string.Empty,
        OrganizerId = t.OrganizerId,
        StartDate = t.StartDate,
        EstimatedEndDate = t.EstimatedEndDate,
        MaxTeams = t.MaxTeams,
        RegisteredTeams = registeredCount,
        MinMembersPerTeam = t.MinMembersPerTeam,
        ScoringSystem = t.ScoringSystem == null ? null! : new ScoringSystemDto
        {
            Type = t.ScoringSystem.Type.ToString(),
            WinPoints = t.ScoringSystem.WinPoints,
            DrawPoints = t.ScoringSystem.DrawPoints,
            LossPoints = t.ScoringSystem.LossPoints,
        },
        CreatedAt = t.CreatedAt,
    };
}
