namespace WebApplication2.Dtos;

public class EventUpdateDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; } = null!;
    public string? BannerImageUrl { get; set; }
}