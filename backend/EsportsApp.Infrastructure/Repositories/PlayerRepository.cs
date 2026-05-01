using EsportsApp.Domain.Entities;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly AppDbContext _context;
    public PlayerRepository(AppDbContext context) => _context = context;

    public Task<Player?> GetByIdAsync(Guid id) =>
        _context.Players.Include(p => p.User).Include(p => p.MainVideogame).FirstOrDefaultAsync(p => p.Id == id);

    public Task<Player?> GetByUsernameAsync(string username) =>
        _context.Players.Include(p => p.User).FirstOrDefaultAsync(p => p.Username == username);

    public Task<bool> UsernameExistsAsync(string username) =>
        _context.Players.AnyAsync(p => p.Username == username);

    public async Task AddAsync(Player player) => await _context.Players.AddAsync(player);

    public Task UpdateAsync(Player player) { _context.Players.Update(player); return Task.CompletedTask; }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
