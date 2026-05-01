using EsportsApp.Application.DTOs.Users;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;

namespace EsportsApp.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IPlayerRepository _players;
    private readonly IOrganizerRepository _organizers;

    public UserService(IUserRepository users, IPlayerRepository players, IOrganizerRepository organizers)
    {
        _users = users;
        _players = players;
        _organizers = organizers;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");

        var dto = new UserProfileDto
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
        };

        if (user.Role == UserRole.Player && user.Player != null)
        {
            dto.Username = user.Player.Username;
            dto.RealName = user.Player.RealName;
            dto.MainVideogameId = user.Player.MainVideogameId;
            dto.MainVideogameName = user.Player.MainVideogame?.Name;
        }
        else if (user.Role == UserRole.Organizer && user.Organizer != null)
        {
            dto.OrganizationName = user.Organizer.OrganizationName;
        }

        return dto;
    }

    public async Task UpdatePlayerProfileAsync(Guid userId, UpdatePlayerProfileRequestDto request)
    {
        var player = await _players.GetByIdAsync(userId)
            ?? throw new NotFoundException("Player not found.");

        player.RealName = request.RealName;

        var user = await _users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            if (await _users.EmailExistsAsync(request.Email))
                throw new ConflictException("Email address is already in use.");
            user.Email = request.Email;
            await _users.UpdateAsync(user);
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword)
                || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new ValidationException("currentPassword", "Current password is incorrect.");

            if (!IsPasswordComplex(request.NewPassword))
                throw new ValidationException("newPassword", "Password must be at least 8 chars with uppercase, lowercase, digit.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _users.UpdateAsync(user);
        }

        await _players.UpdateAsync(player);
        await _players.SaveChangesAsync();
    }

    public async Task UpdateOrganizerProfileAsync(Guid userId, UpdateOrganizerProfileRequestDto request)
    {
        var organizer = await _organizers.GetByIdAsync(userId)
            ?? throw new NotFoundException("Organizer not found.");

        if (organizer.OrganizationName != request.OrganizationName
            && await _organizers.OrganizationNameExistsAsync(request.OrganizationName))
        {
            throw new ConflictException("Organization name already in use.");
        }

        organizer.OrganizationName = request.OrganizationName;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var user = await _users.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");

            if (string.IsNullOrWhiteSpace(request.CurrentPassword)
                || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new ValidationException("currentPassword", "Current password is incorrect.");
            }

            if (!IsPasswordComplex(request.NewPassword))
                throw new ValidationException("newPassword", "Password must be at least 8 chars with uppercase, lowercase, digit.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _users.UpdateAsync(user);
        }

        await _organizers.UpdateAsync(organizer);
        await _organizers.SaveChangesAsync();
    }

    private static bool IsPasswordComplex(string password) =>
        password.Length >= 8 && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit);
}
