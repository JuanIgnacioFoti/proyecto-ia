using EsportsApp.Domain.Entities;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class VideogameRepository : IVideogameRepository
{
    private readonly AppDbContext _context;
    public VideogameRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Videogame>> GetAllAsync() =>
        await _context.Videogames.OrderBy(v => v.Name).ToListAsync();

    public Task<Videogame?> GetByIdAsync(Guid id) =>
        _context.Videogames.FirstOrDefaultAsync(v => v.Id == id);

    public Task<bool> ExistsAsync(Guid id) =>
        _context.Videogames.AnyAsync(v => v.Id == id);
}
