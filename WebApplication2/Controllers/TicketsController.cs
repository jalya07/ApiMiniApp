using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.TicketDto;
using WebApplication2.Helper;

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
 
    // GET /api/tickets [HttpGet]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseModelHelper<IEnumerable<TicketReadDto>>), 200)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
    public async Task<IActionResult> GetAll()
    {
        var tickets = await _db.Tickets.AsNoTracking().ToListAsync();
        return Ok(ResponseModelHelper<IEnumerable<TicketReadDto>>
            .SuccessResult(_mapper.Map<IEnumerable<TicketReadDto>>(tickets)));
    }
 
    // GET /api/tickets/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ResponseModelHelper<TicketReadDto>), 200)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null)
            return NotFound(ResponseModelHelper<string>
                .NotFoundResult($"Ticket {id} not found."));

        return Ok(ResponseModelHelper<TicketReadDto>
            .SuccessResult(_mapper.Map<TicketReadDto>(ticket)));
    }
 
    // POST /api/tickets
    [HttpPost]
    [ProducesResponseType(typeof(ResponseModelHelper<TicketReadDto>), 201)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
    public async Task<IActionResult> Create([FromBody] TicketCreateDto dto, [FromQuery] int eventId)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult(validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
            return NotFound(ResponseModelHelper<string>
                .NotFoundResult($"Event {eventId} not found."));

        var ticket = _mapper.Map<Entities.Ticket>(dto);
        ticket.EventId = eventId;
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ticket.TicketId },
            ResponseModelHelper<TicketReadDto>
                .CreatedResult(_mapper.Map<TicketReadDto>(ticket)));
    }
 
    // PUT /api/tickets/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
    public async Task<IActionResult> Update(int id, [FromBody] TicketUpdateDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult(validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null)
            return NotFound(ResponseModelHelper<string>
                .NotFoundResult($"Ticket {id} not found."));

        _mapper.Map(dto, ticket);
        await _db.SaveChangesAsync();
        return NoContent();
    }
 
    // DELETE /api/tickets/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null)
            return NotFound(ResponseModelHelper<string>
                .NotFoundResult($"Ticket {id} not found."));

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}