using Microsoft.EntityFrameworkCore;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Interfaces;
using TurnOS.Infrastructure.Data;

namespace TurnOS.Infrastructure.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _context;

    public ServiceRepository(AppDbContext context) => _context = context;

    public async Task<Service?> GetByIdAsync(Guid id) =>
        await _context.Services.FindAsync(id);

    public async Task<IEnumerable<Service>> GetByBusinessIdAsync(Guid businessId) =>
        await _context.Services
            .Where(s => s.BusinessId == businessId)
            .ToListAsync();

    public async Task AddAsync(Service service)
    {
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Service service)
    {
        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
    }
}
