using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos;
using WebApplication2.Dtos.OrganizerDto;
using WebApplication2.Entities;
using WebApplication2.Service;

namespace WebApplication2.Controllers;

public class OrganizersController : Controller
{
     private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IValidator<OrganizerCreateDto> _createValidator;
    private readonly IValidator<OrganizerUpdateDto> _updateValidator;
    private readonly IFileUploadService _fileService;
 
    public OrganizersController(
        AppDbContext db,
        IMapper mapper,
        IValidator<OrganizerCreateDto> createValidator,
        IValidator<OrganizerUpdateDto> updateValidator,
        IFileUploadService fileService)
    {
        _db = db;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _fileService = fileService;
    }
 
    // GET /api/organizers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrganizerReadDto>>> GetAll()
    {
        var organizers = await _db.Organizers.AsNoTracking().ToListAsync();
        return Ok(_mapper.Map<IEnumerable<OrganizerReadDto>>(organizers));
    }
 
    // GET /api/organizers/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrganizerReadDto>> GetById(int id)
    {
        var organizer = await _db.Organizers.FindAsync(id);
        if (organizer is null) return NotFound();
        return Ok(_mapper.Map<OrganizerReadDto>(organizer));
    }
 
    // POST /api/organizers
    [HttpPost]
    public async Task<ActionResult<OrganizerReadDto>> Create([FromBody] OrganizerCreateDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
 
        var entity = _mapper.Map<Organizer>(dto);
        _db.Organizers.Add(entity);
        await _db.SaveChangesAsync();
 
        return CreatedAtAction(nameof(GetById), new { id = entity.OrganizerId }, _mapper.Map<OrganizerReadDto>(entity));
    }
 
    // PUT /api/organizers/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] OrganizerUpdateDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
 
        var entity = await _db.Organizers.FindAsync(id);
        if (entity is null) return NotFound();
 
        _mapper.Map(dto, entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
 
    // DELETE /api/organizers/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Organizers.FindAsync(id);
        if (entity is null) return NotFound();
 
        _fileService.DeleteFile(entity.LogoUrl);
        _db.Organizers.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
 
    // POST /api/organizers/{id}/logo
    [HttpPost("{id:int}/logo")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<OrganizerReadDto>> UploadLogo(int id, IFormFile file)
    {
        var entity = await _db.Organizers.FindAsync(id);
        if (entity is null) return NotFound();
 
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");
 
        try
        {
            _fileService.DeleteFile(entity.LogoUrl);
            entity.LogoUrl = await _fileService.SaveFileAsync(file, "logos");
            await _db.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
 
        return Ok(_mapper.Map<OrganizerReadDto>(entity));
    }
 
    // GET /api/organizers/{organizerId}/events
    [HttpGet("{organizerId:int}/events")]
    public async Task<ActionResult<IEnumerable<EventReadDto>>> GetEvents(int organizerId)
    {
        var organizerExists = await _db.Organizers.AnyAsync(o => o.OrganizerId == organizerId);
        if (!organizerExists) return NotFound($"Organizer {organizerId} not found.");
 
        var events = await _db.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == organizerId)
            .ToListAsync();
 
        return Ok(_mapper.Map<IEnumerable<EventReadDto>>(events));
    }
}