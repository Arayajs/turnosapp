using TurnOS.Domain.Enums;

namespace TurnOS.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Notes { get; set; }

    public Guid ClientId { get; set; }
    public User Client { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
