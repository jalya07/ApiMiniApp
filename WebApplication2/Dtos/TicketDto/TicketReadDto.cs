namespace WebApplication2.Dtos.TicketDto;

public class TicketReadDto
{
    public int TicketId { get; set; }
    public int EventId { get; set; }
    public string Type { get; set; } = null!;
    public decimal Price { get; set; }
    public int QuantityAvailable { get; set; }
}