namespace WebApplication2.Dtos.TicketDto;

public class TicketCreateDto
{
    public string Type { get; set; } = null!;
    public decimal Price { get; set; }
    public int QuantityAvailable { get; set; }

}