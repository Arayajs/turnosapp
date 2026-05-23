namespace TurnOS.Application.DTOs.Service;

public class ServiceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
}
