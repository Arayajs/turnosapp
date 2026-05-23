using TurnOS.Application.DTOs.Appointment;
using TurnOS.Application.Interfaces;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Enums;
using TurnOS.Domain.Interfaces;

namespace TurnOS.Application.Services;

public class AppointmentService : IAppointmentService
{
    // Business hours: 09:00 – 19:00. Override per business in a future iteration.
    private static readonly TimeOnly BusinessOpen = new(9, 0);
    private static readonly TimeOnly BusinessClose = new(19, 0);

    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly IBusinessRepository _businessRepo;
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepo;

    public AppointmentService(
        IAppointmentRepository appointmentRepo,
        IServiceRepository serviceRepo,
        IBusinessRepository businessRepo,
        IEmailService emailService,
        IUserRepository userRepo)
    {
        _appointmentRepo = appointmentRepo;
        _serviceRepo = serviceRepo;
        _businessRepo = businessRepo;
        _emailService = emailService;
        _userRepo = userRepo;
    }

    public async Task<AvailableSlotsResponse> GetAvailableSlotsAsync(
        Guid businessId, Guid serviceId, DateOnly date)
    {
        var service = await _serviceRepo.GetByIdAsync(serviceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        if (service.BusinessId != businessId)
            throw new InvalidOperationException("El servicio no pertenece a este negocio.");

        var duration = TimeSpan.FromMinutes(service.DurationMinutes);
        var dayStart = date.ToDateTime(BusinessOpen);
        var dayEnd = date.ToDateTime(BusinessClose);

        var slots = new List<TimeSlotDto>();
        var cursor = dayStart;

        while (cursor + duration <= dayEnd)
        {
            var end = cursor + duration;
            var conflict = await _appointmentRepo.HasConflictAsync(businessId, cursor, end);
            slots.Add(new TimeSlotDto
            {
                StartTime = cursor,
                EndTime = end,
                IsAvailable = !conflict
            });
            cursor += duration;
        }

        return new AvailableSlotsResponse
        {
            BusinessId = businessId,
            ServiceId = serviceId,
            Date = date,
            Slots = slots
        };
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, Guid clientId)
    {
        var service = await _serviceRepo.GetByIdAsync(request.ServiceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        if (service.BusinessId != request.BusinessId)
            throw new InvalidOperationException("El servicio no pertenece a este negocio.");

        var endTime = request.StartTime.AddMinutes(service.DurationMinutes);

        if (request.StartTime < DateTime.UtcNow)
            throw new InvalidOperationException("No puedes agendar citas en el pasado.");

        var hasConflict = await _appointmentRepo.HasConflictAsync(
            request.BusinessId, request.StartTime, endTime);

        if (hasConflict)
            throw new InvalidOperationException("El horario seleccionado ya no está disponible.");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            StartTime = request.StartTime,
            EndTime = endTime,
            Status = AppointmentStatus.Pending,
            Notes = request.Notes,
            ClientId = clientId,
            ServiceId = request.ServiceId,
            BusinessId = request.BusinessId
        };

        await _appointmentRepo.AddAsync(appointment);

        var saved = await _appointmentRepo.GetByIdAsync(appointment.Id)
            ?? throw new InvalidOperationException("Error al guardar la cita.");

        var client = await _userRepo.GetByIdAsync(clientId);
        if (client is not null)
            await _emailService.SendConfirmationAsync(client.Email, saved);

        return MapToResponse(saved);
    }

    public async Task<IEnumerable<AppointmentResponse>> GetMyAppointmentsAsync(Guid clientId)
    {
        var appointments = await _appointmentRepo.GetByClientIdAsync(clientId);
        return appointments.Select(MapToResponse);
    }

    public async Task<IEnumerable<AppointmentResponse>> GetBusinessAppointmentsAsync(
        Guid businessId, Guid requestingUserId, DateOnly? date = null)
    {
        var business = await _businessRepo.GetByIdAsync(businessId)
            ?? throw new KeyNotFoundException("Negocio no encontrado.");

        if (business.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("No tienes acceso a las citas de este negocio.");

        var queryDate = date.HasValue
            ? date.Value.ToDateTime(TimeOnly.MinValue)
            : DateTime.UtcNow;

        var appointments = await _appointmentRepo.GetByBusinessIdAsync(businessId, queryDate);
        return appointments.Select(MapToResponse);
    }

    public async Task<AppointmentResponse> UpdateStatusAsync(
        Guid id, UpdateAppointmentStatusRequest request, Guid requestingUserId)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Cita no encontrada.");

        var business = await _businessRepo.GetByIdAsync(appointment.BusinessId)
            ?? throw new KeyNotFoundException("Negocio no encontrado.");

        if (business.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("No tienes permiso para modificar esta cita.");

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("No se puede cambiar el estado de una cita cancelada.");

        appointment.Status = request.Status;
        await _appointmentRepo.UpdateAsync(appointment);

        return MapToResponse(appointment);
    }

    public async Task CancelAsync(Guid id, Guid clientId)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Cita no encontrada.");

        if (appointment.ClientId != clientId)
            throw new UnauthorizedAccessException("Solo puedes cancelar tus propias citas.");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("No se puede cancelar una cita completada.");

        appointment.Status = AppointmentStatus.Cancelled;
        await _appointmentRepo.UpdateAsync(appointment);

        var client = await _userRepo.GetByIdAsync(clientId);
        if (client is not null)
            await _emailService.SendCancellationAsync(client.Email, appointment);
    }

    private static AppointmentResponse MapToResponse(Appointment a) => new()
    {
        Id = a.Id,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status.ToString(),
        Notes = a.Notes,
        ClientId = a.ClientId,
        ClientName = a.Client?.FullName ?? string.Empty,
        ServiceId = a.ServiceId,
        ServiceName = a.Service?.Name ?? string.Empty,
        DurationMinutes = a.Service?.DurationMinutes ?? 0,
        Price = a.Service?.Price ?? 0,
        BusinessId = a.BusinessId,
        BusinessName = a.Business?.Name ?? string.Empty,
        CreatedAt = a.CreatedAt
    };
}
