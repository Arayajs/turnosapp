using System.ComponentModel.DataAnnotations;

namespace TurnOS.Application.DTOs.Service;

public class UpdateServiceRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(5, 480)]
    public int DurationMinutes { get; set; }

    [Range(0, 100000)]
    public decimal Price { get; set; }
}
