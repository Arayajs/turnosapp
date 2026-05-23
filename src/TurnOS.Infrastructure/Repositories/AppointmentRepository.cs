using Microsoft.EntityFrameworkCore;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Interfaces;
using TurnOS.Infrastructure.Data;

namespace TurnOS.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context) => _context = context;

    public async Task<Appointment?> GetByIdAsync(Guid id) =>
        await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Include(a => a.Business)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<Appointment>> GetByClientIdAsync(Guid clientId) =>
        await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Business)
            .Where(a => a.ClientId == clientId)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<Appointment>> GetByBusinessIdAsync(Guid businessId, DateTime date) =>
        await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Where(a => a.BusinessId == businessId && a.StartTime.Date == date.Date)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

    public async Task<bool> HasConflictAsync(Guid businessId, DateTime startTime, DateTime endTime) =>
        await _context.Appointments
            .AnyAsync(a =>
                a.BusinessId == businessId &&
                a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                a.StartTime < endTime &&
                a.EndTime > startTime);

    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Appointment appointment)
    {
        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
    }
}
