using Microsoft.EntityFrameworkCore;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Interfaces;
using TurnOS.Infrastructure.Data;

namespace TurnOS.Infrastructure.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly AppDbContext _context;

    public BusinessRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Business>> GetAllAsync() =>
        await _context.Businesses.Include(b => b.Owner).ToListAsync();

    public async Task<Business?> GetByIdAsync(Guid id) =>
        await _context.Businesses
            .Include(b => b.Owner)
            .Include(b => b.Services)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IEnumerable<Business>> GetByOwnerIdAsync(Guid ownerId) =>
        await _context.Businesses
            .Where(b => b.OwnerId == ownerId)
            .ToListAsync();

    public async Task AddAsync(Business business)
    {
        await _context.Businesses.AddAsync(business);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Business business)
    {
        _context.Businesses.Update(business);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Business business)
    {
        _context.Businesses.Remove(business);
        await _context.SaveChangesAsync();
    }
}
