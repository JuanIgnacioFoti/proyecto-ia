using EsportsApp.Domain.Entities;

namespace EsportsApp.Infrastructure.Interfaces;

public interface IVideogameRepository
{
    Task<IEnumerable<Videogame>> GetAllAsync();
    Task<Videogame?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
