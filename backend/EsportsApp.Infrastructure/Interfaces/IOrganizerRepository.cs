using EsportsApp.Domain.Entities;

namespace EsportsApp.Infrastructure.Interfaces;

public interface IOrganizerRepository
{
    Task<Organizer?> GetByIdAsync(Guid id);
    Task<bool> OrganizationNameExistsAsync(string name);
    Task AddAsync(Organizer organizer);
    Task UpdateAsync(Organizer organizer);
    Task SaveChangesAsync();
}
