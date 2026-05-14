using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos;
using WebApplication2.Dtos.OrganizerDto;
using WebApplication2.Dtos.TicketDto;
using WebApplication2.Entities;
using WebApplication2.Service;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : Controller
{
   private readonly AppDbContext _db;
     private readonly IMapper _mapper;
     private readonly IValidator<EventCreateDto> _createValidator;
     private readonly IValidator<EventUpdateDto> _updateValidator;
     private readonly IFileUploadService _fileService;
  
     public EventsController(
         AppDbContext db,
         IMapper mapper,
         IValidator<EventCreateDto> createValidator,
         IValidator<EventUpdateDto> updateValidator,
         IFileUploadService fileService)
     {
         _db = db;
         _mapper = mapper;
         _createValidator = createValidator;
         _updateValidator = updateValidator;
         _fileService = fileService;
     }
  
     // GET /api/events
     [HttpGet]
     public async Task<ActionResult<IEnumerable<EventReadDto>>> GetAll()
     {
         var events = await _db.Events.AsNoTracking().ToListAsync();
         return Ok(_mapper.Map<IEnumerable<EventReadDto>>(events));
         
     }
  
     // GET /api/events/{id}
     [HttpGet("{id:int}")]
     public async Task<ActionResult<EventReadDto>> GetById(int id)
     {
         var ev = await _db.Events.FindAsync(id);
         if (ev is null) return NotFound();
         return Ok(_mapper.Map<EventReadDto>(ev));
     }
  
     // POST /api/events
     [HttpPost]
     public async Task<ActionResult<EventReadDto>> Create([FromBody] EventCreateDto dto)
     {
         var validation = await _createValidator.ValidateAsync(dto);
         if (!validation.IsValid)
             return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
  
         var organizerExists = await _db.Organizers.AnyAsync(o => o.OrganizerId == dto.OrganizerId);
         if (!organizerExists)
             return BadRequest($"Organizer with Id {dto.OrganizerId} does not exist.");
  
         var entity = _mapper.Map<Event>(dto);
         _db.Events.Add(entity);
         await _db.SaveChangesAsync();
  
         return CreatedAtAction(nameof(GetById), new { id = entity.EventId }, _mapper.Map<EventReadDto>(entity));
     }
  
     // PUT /api/events/{id}
     [HttpPut("{id:int}")]
     public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto)
     {
         var validation = await _updateValidator.ValidateAsync(dto);
         if (!validation.IsValid)
             return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
  
         var entity = await _db.Events.FindAsync(id);
         if (entity is null) return NotFound();
  
         var organizerExists = await _db.Organizers.AnyAsync(o => o.OrganizerId == dto.OrganizerId);
         if (!organizerExists)
             return BadRequest($"Organizer with Id {dto.OrganizerId} does not exist.");
  
         _mapper.Map(dto, entity);
         await _db.SaveChangesAsync();
         return NoContent();
     }
  
     // DELETE /api/events/{id}
     [HttpDelete("{id:int}")]
     public async Task<IActionResult> Delete(int id)
     {
         var entity = await _db.Events.FindAsync(id);
         if (entity is null) return NotFound();
     
         _fileService.DeleteFile(entity.BannerImageUrl);
         _db.Events.Remove(entity);
         await _db.SaveChangesAsync();
         return NoContent();
     }
     
     // POST /api/events/{id}/banner
     [HttpPost("{id:int}/banner")]
     [Consumes("multipart/form-data")]
     public async Task<ActionResult<EventReadDto>> UploadBanner(int id, IFormFile file)
     {
         var entity = await _db.Events.FindAsync(id);
         if (entity is null) return NotFound();
     
         if (file is null || file.Length == 0)
             return BadRequest("No file provided.");
     
         try
         {
             _fileService.DeleteFile(entity.BannerImageUrl);
             entity.BannerImageUrl = await _fileService.SaveFileAsync(file, "banners");
             await _db.SaveChangesAsync();
         }
         catch (InvalidOperationException ex)
         {
             return BadRequest(ex.Message);
         }
     
         return Ok(_mapper.Map<EventReadDto>(entity));
     }
  
     // GET /api/events/{eventId}/tickets
     [HttpGet("{eventId:int}/tickets")]
     public async Task<ActionResult<IEnumerable<TicketReadDto>>> GetTickets(int eventId)
     {
         var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
         if (!eventExists) return NotFound($"Event {eventId} not found.");
     
         var tickets = await _db.Tickets
             .AsNoTracking()
             .Where(t => t.EventId == eventId)
             .ToListAsync();
     
         return Ok(_mapper.Map<IEnumerable<TicketReadDto>>(tickets));
     }
     
     // POST /api/events/{eventId}/tickets
     [HttpPost("{eventId:int}/tickets")]
     public async Task<ActionResult<TicketReadDto>> CreateTicket(
         int eventId,
         [FromBody] TicketCreateDto dto,
         [FromServices] IValidator<TicketCreateDto> validator)
     {
         var validation = await validator.ValidateAsync(dto);
         if (!validation.IsValid)
             return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
     
         var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
         if (!eventExists) return NotFound($"Event {eventId} not found.");
     
         var ticket = _mapper.Map<Ticket>(dto);
         ticket.EventId = eventId;
         _db.Tickets.Add(ticket);
         await _db.SaveChangesAsync();
     
         return CreatedAtAction(
             nameof(TicketsController.GetById),
             "Tickets",
             new { id = ticket.TicketId },
             _mapper.Map<TicketReadDto>(ticket));
     }
     
     // GET /api/events/{eventId}/organizer
     [HttpGet("{eventId:int}/organizer")]
     public async Task<ActionResult<OrganizerReadDto>> GetOrganizer(int eventId)
     {
         var ev = await _db.Events
             .AsNoTracking()
             .Include(e => e.Organizer)
             .FirstOrDefaultAsync(e => e.EventId == eventId);
     
         if (ev is null) return NotFound($"Event {eventId} not found.");
     
         return Ok(_mapper.Map<OrganizerReadDto>(ev.Organizer));
     }
}
