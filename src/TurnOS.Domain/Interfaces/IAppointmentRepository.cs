using TurnOS.Domain.Entities;

namespace TurnOS.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Appointment>> GetByClientIdAsync(Guid clientId);
    Task<IEnumerable<Appointment>> GetByBusinessIdAsync(Guid businessId, DateTime date);
    Task<bool> HasConflictAsync(Guid businessId, DateTime startTime, DateTime endTime);
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(Appointment appointment);
}
