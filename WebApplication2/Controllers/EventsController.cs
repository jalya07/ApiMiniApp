using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos;
using WebApplication2.Dtos.OrganizerDto;
using WebApplication2.Dtos.TicketDto;
using WebApplication2.Entities;
using WebApplication2.Helper;
using WebApplication2.Service;

namespace WebApplication2.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
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
     // [HttpGet]
     // public async Task<ActionResult<IEnumerable<EventReadDto>>> GetAll()
     // {
     //     var events = await _db.Events.AsNoTracking().ToListAsync();
     //     return Ok(_mapper.Map<IEnumerable<EventReadDto>>(events));
     //     
     // }
     [HttpGet]
     [ProducesResponseType(typeof(ResponseModelHelper<IEnumerable<EventReadDto>>), 200)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
     public async Task<IActionResult> GetAll()
     {
         var events = await _db.Events.AsNoTracking().ToListAsync();
         return Ok(ResponseModelHelper<IEnumerable<EventReadDto>>
             .SuccessResult(_mapper.Map<IEnumerable<EventReadDto>>(events)));
     }
  
     // GET /api/events/{id}
     // [HttpGet("{id:int}")]
     // public async Task<ActionResult<EventReadDto>> GetById(int id)
     // {
     //     var ev = await _db.Events.FindAsync(id);
     //     if (ev is null) return NotFound();
     //     return Ok(_mapper.Map<EventReadDto>(ev));
     // }
     [HttpGet("{id:int}")]
     [ProducesResponseType(typeof(ResponseModelHelper<EventReadDto>), 200)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
     public async Task<IActionResult> GetById(int id)
     {
         var ev = await _db.Events.FindAsync(id);
         if (ev is null)
             return NotFound(ResponseModelHelper<string>
                 .NotFoundResult($"Event {id} not found."));

         return Ok(ResponseModelHelper<EventReadDto>
             .SuccessResult(_mapper.Map<EventReadDto>(ev)));
     }
  
     // POST /api/events
     [HttpPost]
     [ProducesResponseType(typeof(ResponseModelHelper<EventReadDto>), 201)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
     public async Task<ActionResult<EventReadDto>> Create([FromBody] EventCreateDto dto)
     {
         var validation = await _createValidator.ValidateAsync(dto);
         if (!validation.IsValid)
             return BadRequest(ResponseModelHelper<string>
                 .BadRequestResult(validation.Errors.Select(e => e.ErrorMessage).ToArray()));

         var organizerExists = await _db.Organizers.AnyAsync(o => o.OrganizerId == dto.OrganizerId);
         if (!organizerExists)
             return BadRequest(ResponseModelHelper<string>
                 .BadRequestResult($"Organizer with Id {dto.OrganizerId} does not exist."));

         var entity = _mapper.Map<Event>(dto);
         _db.Events.Add(entity);
         await _db.SaveChangesAsync();

         return CreatedAtAction(nameof(GetById), new { id = entity.EventId },
             ResponseModelHelper<EventReadDto>
                 .CreatedResult(_mapper.Map<EventReadDto>(entity)));
     }
  
     // PUT /api/events/{id}
     [HttpPut("{id:int}")]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 204)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
     public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto)
     {
         var validation = await _updateValidator.ValidateAsync(dto);
         if (!validation.IsValid)
             return BadRequest(ResponseModelHelper<string>
                 .BadRequestResult(validation.Errors.Select(e => e.ErrorMessage).ToArray()));

         var entity = await _db.Events.FindAsync(id);
         if (entity is null)
             return NotFound(ResponseModelHelper<string>
                 .NotFoundResult($"Event {id} not found."));

         var organizerExists = await _db.Organizers.AnyAsync(o => o.OrganizerId == dto.OrganizerId);
         if (!organizerExists)
             return BadRequest(ResponseModelHelper<string>
                 .BadRequestResult($"Organizer with Id {dto.OrganizerId} does not exist."));

         _mapper.Map(dto, entity);
         await _db.SaveChangesAsync();
         return NoContent();
     }
  
     // DELETE /api/events/{id}
     [HttpDelete("{id:int}")]
     [ProducesResponseType(204)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
     public async Task<IActionResult> Delete(int id)
     {
         var entity = await _db.Events.FindAsync(id);
         if (entity is null)
             return NotFound(ResponseModelHelper<string>
                 .NotFoundResult($"Event {id} not found."));

         _fileService.DeleteFile(entity.BannerImageUrl);
         _db.Events.Remove(entity);
         await _db.SaveChangesAsync();
         return NoContent();
     }
     
     // POST /api/events/{id}/banner
     [HttpPost("{id:int}/banner")]
     [Consumes("multipart/form-data")]
     [ProducesResponseType(typeof(ResponseModelHelper<EventReadDto>), 200)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
     public async Task<ActionResult<EventReadDto>> UploadBanner(int id, IFormFile file)
     {
         var entity = await _db.Events.FindAsync(id);
         if (entity is null)
             return NotFound(ResponseModelHelper<string>.NotFoundResult($"Event {id} not found."));

         if (file is null || file.Length == 0)
             return BadRequest(ResponseModelHelper<string>.BadRequestResult("No file provided."));

         try
         {
             _fileService.DeleteFile(entity.BannerImageUrl);
             entity.BannerImageUrl = await _fileService.SaveFileAsync(file, "banners");
             await _db.SaveChangesAsync();
         }
         catch (InvalidOperationException ex)
         {
             return BadRequest(ResponseModelHelper<string>.BadRequestResult(ex.Message));
         }

         return Ok(ResponseModelHelper<EventReadDto>
             .SuccessResult(_mapper.Map<EventReadDto>(entity)));
     }

  
     // GET /api/events/{eventId}/tickets
     [HttpGet("{eventId:int}/tickets")]
     [ProducesResponseType(typeof(ResponseModelHelper<IEnumerable<TicketReadDto>>), 200)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
     public async Task<ActionResult<IEnumerable<TicketReadDto>>> GetTickets(int eventId)
     {
         var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
         if (!eventExists)
             return NotFound(ResponseModelHelper<string>.NotFoundResult($"Event {eventId} not found."));

         var tickets = await _db.Tickets
             .AsNoTracking()
             .Where(t => t.EventId == eventId)
             .ToListAsync();

         return Ok(ResponseModelHelper<IEnumerable<TicketReadDto>>
             .SuccessResult(_mapper.Map<IEnumerable<TicketReadDto>>(tickets)));
     }
     
     // POST /api/events/{eventId}/tickets
     [HttpPost("{eventId:int}/tickets")]
     [ProducesResponseType(typeof(ResponseModelHelper<TicketReadDto>), 201)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
     public async Task<ActionResult<TicketReadDto>> CreateTicket(
         int eventId,
         [FromBody] TicketCreateDto dto,
         [FromServices] IValidator<TicketCreateDto> validator)
     {
         var validation = await validator.ValidateAsync(dto);
         if (!validation.IsValid)
             return BadRequest(ResponseModelHelper<string>
                 .BadRequestResult(validation.Errors.Select(e => e.ErrorMessage).ToArray()));

         var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
         if (!eventExists)
             return NotFound(ResponseModelHelper<string>.NotFoundResult($"Event {eventId} not found."));

         var ticket = _mapper.Map<Ticket>(dto);
         ticket.EventId = eventId;
         _db.Tickets.Add(ticket);
         await _db.SaveChangesAsync();

         return CreatedAtAction(
             nameof(TicketsController.GetById),
             "Tickets",
             new { id = ticket.TicketId },
             ResponseModelHelper<TicketReadDto>
                 .CreatedResult(_mapper.Map<TicketReadDto>(ticket)));
     }
     
     // GET /api/events/{eventId}/organizer
     [HttpGet("{eventId:int}/organizer")]
     [ProducesResponseType(typeof(ResponseModelHelper<OrganizerReadDto>), 200)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 404)]
     [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
     public async Task<ActionResult<OrganizerReadDto>> GetOrganizer(int eventId)
     {
         var ev = await _db.Events
             .AsNoTracking()
             .Include(e => e.Organizer)
             .FirstOrDefaultAsync(e => e.EventId == eventId);

         if (ev is null)
             return NotFound(ResponseModelHelper<string>.NotFoundResult($"Event {eventId} not found."));

         return Ok(ResponseModelHelper<OrganizerReadDto>
             .SuccessResult(_mapper.Map<OrganizerReadDto>(ev.Organizer)));
     }
}
