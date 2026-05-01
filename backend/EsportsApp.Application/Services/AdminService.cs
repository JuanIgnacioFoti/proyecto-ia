using EsportsApp.Application.DTOs.Users;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;

namespace EsportsApp.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _users;
    private readonly ITournamentRepository _tournaments;

    public AdminService(IUserRepository users, ITournamentRepository tournaments)
    {
        _users = users;
        _tournaments = tournaments;
    }

    public async Task<IEnumerable<UserSummaryDto>> GetUsersAsync()
    {
        var users = await _users.GetAllAsync();
        return users.Select(u => new UserSummaryDto
        {
            UserId = u.Id,
            Email = u.Email,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            DisplayName = u.Player?.Username ?? u.Organizer?.OrganizationName,
        });
    }

    public async Task SetRoleAsync(Guid adminId, Guid targetUserId, string role)
    {
        var user = await _users.GetByIdAsync(targetUserId)
            ?? throw new NotFoundException("User not found.");

        if (user.Id == adminId)
            throw new ForbiddenException("Cannot change your own role.");

        if (!Enum.TryParse<UserRole>(role, true, out var newRole))
            throw new ValidationException("role", $"Invalid role: {role}");

        user.Role = newRole;
        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();
    }

    public async Task SuspendUserAsync(Guid adminId, Guid targetUserId)
    {
        var user = await _users.GetByIdAsync(targetUserId)
            ?? throw new NotFoundException("User not found.");

        if (user.Role == UserRole.Admin)
            throw new ForbiddenException("Cannot suspend another admin.");

        user.IsActive = false;
        await _users.UpdateAsync(user);

        // Suspend non-completed organizer tournaments
        if (user.Role == UserRole.Organizer && user.Organizer != null)
        {
            var orgTournaments = await _tournaments.GetByOrganizerAsync(user.Organizer.Id);
            foreach (var t in orgTournaments)
            {
                if (t.Status != TournamentStatus.Completed && t.Status != TournamentStatus.Suspended)
                {
                    t.PreSuspensionStatus = t.Status;
                    t.Status = TournamentStatus.Suspended;
                    await _tournaments.UpdateAsync(t);
                }
            }
        }

        await _users.SaveChangesAsync();
    }

    public async Task ReinstateUserAsync(Guid adminId, Guid targetUserId)
    {
        var user = await _users.GetByIdAsync(targetUserId)
            ?? throw new NotFoundException("User not found.");

        user.IsActive = true;
        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();
    }
}
