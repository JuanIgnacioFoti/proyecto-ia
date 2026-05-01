using EsportsApp.Domain.Entities;

namespace EsportsApp.Infrastructure.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id);
    Task<Player?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
    Task AddAsync(Player player);
    Task UpdateAsync(Player player);
    Task SaveChangesAsync();
}
