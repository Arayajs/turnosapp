namespace TurnOS.Domain.Entities;

public class Business
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public ICollection<Service> Services { get; set; } = new List<Service>();
}
