using TurnOS.Domain.Entities;

namespace TurnOS.Domain.Interfaces;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id);
    Task<IEnumerable<Service>> GetByBusinessIdAsync(Guid businessId);
    Task AddAsync(Service service);
    Task UpdateAsync(Service service);
    Task DeleteAsync(Service service);
}
