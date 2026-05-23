using TurnOS.Domain.Entities;

namespace TurnOS.Domain.Interfaces;

public interface IBusinessRepository
{
    Task<IEnumerable<Business>> GetAllAsync();
    Task<Business?> GetByIdAsync(Guid id);
    Task<IEnumerable<Business>> GetByOwnerIdAsync(Guid ownerId);
    Task AddAsync(Business business);
    Task UpdateAsync(Business business);
    Task DeleteAsync(Business business);
}
