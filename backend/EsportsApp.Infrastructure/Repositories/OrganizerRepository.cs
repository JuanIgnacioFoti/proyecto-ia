using EsportsApp.Domain.Entities;
using EsportsApp.Infrastructure.Data;
using EsportsApp.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EsportsApp.Infrastructure.Repositories;

public class OrganizerRepository : IOrganizerRepository
{
    private readonly AppDbContext _context;
    public OrganizerRepository(AppDbContext context) => _context = context;

    public Task<Organizer?> GetByIdAsync(Guid id) =>
        _context.Organizers.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);

    public Task<bool> OrganizationNameExistsAsync(string name) =>
        _context.Organizers.AnyAsync(o => o.OrganizationName == name);

    public async Task AddAsync(Organizer organizer) => await _context.Organizers.AddAsync(organizer);

    public Task UpdateAsync(Organizer organizer) { _context.Organizers.Update(organizer); return Task.CompletedTask; }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
