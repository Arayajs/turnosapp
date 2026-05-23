using System.ComponentModel.DataAnnotations;
using TurnOS.Domain.Enums;

namespace TurnOS.Application.DTOs.Appointment;

public class UpdateAppointmentStatusRequest
{
    [Required]
    public AppointmentStatus Status { get; set; }
}
