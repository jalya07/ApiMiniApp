namespace WebApplication2.Entities;

public class Event
{
    public int EventId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; } = null!;
    public string? BannerImageUrl { get; set; }
 
    public int OrganizerId { get; set; }
    public Organizer Organizer { get; set; } = null!;
 
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}