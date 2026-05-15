using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.TicketDto;

namespace WebApplication2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IValidator<TicketCreateDto> _createValidator;
    private readonly IValidator<TicketUpdateDto> _updateValidator;
 
    public TicketsController(
        AppDbContext db,
        IMapper mapper,
        IValidator<TicketCreateDto> createValidator,
        IValidator<TicketUpdateDto> updateValidator)
    {
        _db = db;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
 
    // GET /api/tickets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketReadDto>>> GetAll()
    {
        var tickets = await _db.Tickets.AsNoTracking().ToListAsync();
        return Ok(_mapper.Map<IEnumerable<TicketReadDto>>(tickets));
    }
 
    // GET /api/tickets/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketReadDto>> GetById(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();
        return Ok(_mapper.Map<TicketReadDto>(ticket));
    }
 
    // POST /api/tickets
    [HttpPost]
    public async Task<ActionResult<TicketReadDto>> Create([FromBody] TicketCreateDto dto,
        [FromQuery] int eventId)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
 
        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists) return BadRequest($"Event {eventId} not found.");
 
        var ticket = _mapper.Map<Entities.Ticket>(dto);
        ticket.EventId = eventId;
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
 
        return CreatedAtAction(nameof(GetById), new { id = ticket.TicketId }, _mapper.Map<TicketReadDto>(ticket));
    }
 
    // PUT /api/tickets/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TicketUpdateDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
 
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();
 
        _mapper.Map(dto, ticket);
        await _db.SaveChangesAsync();
        return NoContent();
    }
 
    // DELETE /api/tickets/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();
 
        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}