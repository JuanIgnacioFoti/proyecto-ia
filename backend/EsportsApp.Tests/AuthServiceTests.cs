using EsportsApp.Application.DTOs.Auth;
using EsportsApp.Application.Exceptions;
using EsportsApp.Application.Interfaces;
using EsportsApp.Application.Services;
using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EsportsApp.Tests;

[TestClass]
public class AuthServiceTests
{
    private Mock<IUserRepository> _users = null!;
    private Mock<IPlayerRepository> _players = null!;
    private Mock<IOrganizerRepository> _organizers = null!;
    private Mock<IVideogameRepository> _videogames = null!;
    private Mock<IJwtTokenService> _jwt = null!;
    private AuthService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _users = new Mock<IUserRepository>();
        _players = new Mock<IPlayerRepository>();
        _organizers = new Mock<IOrganizerRepository>();
        _videogames = new Mock<IVideogameRepository>();
        _jwt = new Mock<IJwtTokenService>();
        _sut = new AuthService(_users.Object, _players.Object, _organizers.Object, _videogames.Object, _jwt.Object);
    }

    [TestMethod]
    public async Task RegisterPlayer_DuplicateEmail_ThrowsConflict()
    {
        _users.Setup(r => r.EmailExistsAsync("test@test.com")).ReturnsAsync(true);
        var req = new RegisterPlayerRequestDto { Email = "test@test.com", Password = "Pass1234", Username = "user1", VideogameId = Guid.NewGuid() };
        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.RegisterPlayerAsync(req));
    }

    [TestMethod]
    public async Task RegisterPlayer_DuplicateUsername_ThrowsConflict()
    {
        _users.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _players.Setup(r => r.UsernameExistsAsync("user1")).ReturnsAsync(true);
        var req = new RegisterPlayerRequestDto { Email = "new@test.com", Password = "Pass1234", Username = "user1", VideogameId = Guid.NewGuid() };
        await Assert.ThrowsExceptionAsync<ConflictException>(() => _sut.RegisterPlayerAsync(req));
    }

    [TestMethod]
    public async Task RegisterPlayer_WeakPassword_ThrowsValidation()
    {
        _users.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _players.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        var req = new RegisterPlayerRequestDto { Email = "new@test.com", Password = "password", Username = "user1", VideogameId = Guid.NewGuid() };
        await Assert.ThrowsExceptionAsync<ValidationException>(() => _sut.RegisterPlayerAsync(req));
    }

    [TestMethod]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"), Role = UserRole.Player, IsActive = true };
        _users.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        var req = new LoginRequestDto { Email = "test@test.com", Password = "WrongPassword" };
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(req));
    }

    [TestMethod]
    public async Task Login_InactiveUser_ThrowsUnauthorized()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"), Role = UserRole.Player, IsActive = false };
        _users.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        var req = new LoginRequestDto { Email = "test@test.com", Password = "Pass1234" };
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(req));
    }
}
