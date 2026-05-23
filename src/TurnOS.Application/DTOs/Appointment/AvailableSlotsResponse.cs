namespace TurnOS.Application.DTOs.Appointment;

public class AvailableSlotsResponse
{
    public Guid BusinessId { get; set; }
    public Guid ServiceId { get; set; }
    public DateOnly Date { get; set; }
    public List<TimeSlotDto> Slots { get; set; } = new();
}

public class TimeSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
