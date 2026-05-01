using EsportsApp.Application.DTOs.Videogames;
using EsportsApp.Application.Interfaces;
using EsportsApp.Infrastructure.Interfaces;

namespace EsportsApp.Application.Services;

public class VideogameService : IVideogameService
{
    private readonly IVideogameRepository _videogames;
    public VideogameService(IVideogameRepository videogames) => _videogames = videogames;

    public async Task<IEnumerable<VideogameDto>> GetAllAsync()
    {
        var games = await _videogames.GetAllAsync();
        return games.Select(g => new VideogameDto { Id = g.Id, Name = g.Name });
    }
}
