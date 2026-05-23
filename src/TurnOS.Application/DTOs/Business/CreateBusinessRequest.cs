using System.ComponentModel.DataAnnotations;

namespace TurnOS.Application.DTOs.Business;

public class CreateBusinessRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
}
