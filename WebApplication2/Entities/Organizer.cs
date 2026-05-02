namespace WebApplication2.Entities;

public class Organizer
{
    public int OrganizerId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? LogoUrl { get; set; }
 
    public ICollection<Event> Events { get; set; } = new List<Event>();
}