using System.ComponentModel.DataAnnotations;

namespace TurnOS.Application.DTOs.Appointment;

public class CreateAppointmentRequest
{
    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public Guid BusinessId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
