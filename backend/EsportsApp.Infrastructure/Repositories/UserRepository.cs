using EsportsApp.Domain.Entities;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public Task<User?> GetByIdAsync(Guid id) =>
        _context.Users.Include(u => u.Player).Include(u => u.Organizer).FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        _context.Users.Include(u => u.Player).Include(u => u.Organizer)
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public Task<bool> EmailExistsAsync(string email) =>
        _context.Users.AnyAsync(u => u.Email == email.ToLower());

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users.Include(u => u.Player).Include(u => u.Organizer).OrderBy(u => u.CreatedAt).ToListAsync();

    public async Task AddAsync(User user)
    {
        user.Email = user.Email.ToLower();
        await _context.Users.AddAsync(user);
    }

    public Task UpdateAsync(User user) { _context.Users.Update(user); return Task.CompletedTask; }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
