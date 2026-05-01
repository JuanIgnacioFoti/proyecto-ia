using EsportsApp.Domain.Entities;
using EsportsApp.Domain.Enums;

namespace EsportsApp.Infrastructure.Interfaces;

public interface ITournamentRepository
{
    Task<Tournament?> GetByIdAsync(Guid id);
    Task<IEnumerable<Tournament>> GetPublicListAsync();
    Task<IEnumerable<Tournament>> GetByOrganizerAsync(Guid organizerId);
    Task AddAsync(Tournament tournament);
    Task UpdateAsync(Tournament tournament);
    Task DeleteAsync(Tournament tournament);
    Task SaveChangesAsync();
}
