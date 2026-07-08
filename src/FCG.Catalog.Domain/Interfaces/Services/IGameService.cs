using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Domain.Interfaces.Services;

public interface IGameService
{
    Task<Game> CreateAsync(string title, string description, decimal price, string genre);
    Task<Game?> GetByIdAsync(Guid id);
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> UpdateAsync(Guid id, string title, string description, decimal price, string genre);
    Task<bool> DeleteAsync(Guid id);
}