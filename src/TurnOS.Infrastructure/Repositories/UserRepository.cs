using Microsoft.EntityFrameworkCore;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Interfaces;
using TurnOS.Infrastructure.Data;

namespace TurnOS.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _context.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken) =>
        await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string email) =>
        await _context.Users.AnyAsync(u => u.Email == email);
}
