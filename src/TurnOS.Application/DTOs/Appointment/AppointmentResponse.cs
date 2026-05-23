namespace TurnOS.Application.DTOs.Appointment;

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;

    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }

    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
