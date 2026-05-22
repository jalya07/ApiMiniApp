namespace WebApplication2.Entities;

public class UserEvent
{
    public string AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;
    
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
}