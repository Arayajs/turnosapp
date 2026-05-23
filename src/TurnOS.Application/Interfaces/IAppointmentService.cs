using TurnOS.Application.DTOs.Appointment;

namespace TurnOS.Application.Interfaces;

public interface IAppointmentService
{
    Task<AvailableSlotsResponse> GetAvailableSlotsAsync(Guid businessId, Guid serviceId, DateOnly date);
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, Guid clientId);
    Task<IEnumerable<AppointmentResponse>> GetMyAppointmentsAsync(Guid clientId);
    Task<IEnumerable<AppointmentResponse>> GetBusinessAppointmentsAsync(Guid businessId, Guid requestingUserId, DateOnly? date = null);
    Task<AppointmentResponse> UpdateStatusAsync(Guid id, UpdateAppointmentStatusRequest request, Guid requestingUserId);
    Task CancelAsync(Guid id, Guid clientId);
}
