using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication2.Controllers;
using WebApplication2.Data;
using WebApplication2.Dtos;
using WebApplication2.Entities;
using WebApplication2.Service;
using Xunit;

namespace WebApplication2.Tests.ControllerTests;

public class EventsControllerTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IValidator<EventCreateDto>> _createValidator;
    private readonly Mock<IValidator<EventUpdateDto>> _updateValidator;
    private readonly Mock<IFileUploadService> _fileService;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        _mapper = new Mock<IMapper>();
        _createValidator = new Mock<IValidator<EventCreateDto>>();
        _updateValidator = new Mock<IValidator<EventUpdateDto>>();
        _fileService = new Mock<IFileUploadService>();

        _controller = new EventsController(
            _db,
            _mapper.Object,
            _createValidator.Object,
            _updateValidator.Object,
            _fileService.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenValidationFails()
    {
        var dto = new EventCreateDto { Title = "" };
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Title", "Title is required.")
        });
        _createValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(validationResult);

        var result = await _controller.Create(dto);

        // ActionResult<T> содержит Result внутри
        var actionResult = result as ActionResult<EventReadDto>;
        actionResult!.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValidData()
    {
        // Arrange
        _db.Organizers.Add(new Organizer 
        { 
            OrganizerId = 1, 
            Name = "Test Org", 
            Email = "org@test.com" 
        });
        await _db.SaveChangesAsync();

        var dto = new EventCreateDto
        {
            Title = "AI Summit",
            Date = DateTime.UtcNow.AddDays(10),
            Location = "Baku",
            OrganizerId = 1
        };

        // entity с полными обязательными полями
        var entity = new Event 
        { 
            EventId = 1, 
            Title = "AI Summit",
            Date = DateTime.UtcNow.AddDays(10),
            Location = "Baku",
            OrganizerId = 1
        };
        var readDto = new EventReadDto { EventId = 1, Title = "AI Summit" };

        _createValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());
        _mapper.Setup(m => m.Map<Event>(dto)).Returns(entity);
        _mapper.Setup(m => m.Map<EventReadDto>(entity)).Returns(readDto);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var actionResult = result as ActionResult<EventReadDto>;
        actionResult!.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithListOfEvents()
    {
        _db.Events.AddRange(
            new Event { Title = "Event 1", Date = DateTime.UtcNow.AddDays(5), Location = "Baku", OrganizerId = 1 },
            new Event { Title = "Event 2", Date = DateTime.UtcNow.AddDays(10), Location = "Baku", OrganizerId = 1 }
        );
        await _db.SaveChangesAsync();

        _mapper.Setup(m => m.Map<IEnumerable<EventReadDto>>(It.IsAny<IEnumerable<Event>>()))
            .Returns(new List<EventReadDto> { new(), new() });

        var result = await _controller.GetAll();

        var actionResult = result as ActionResult<IEnumerable<EventReadDto>>;
        actionResult!.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        var result = await _controller.GetById(999);

        var actionResult = result as ActionResult<EventReadDto>;
        actionResult!.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenEventExists()
    {
        // Arrange
        var ev = new Event { Title = "Test", Date = DateTime.UtcNow.AddDays(5), Location = "Baku", OrganizerId = 1 };
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        // Act
        var result = await _controller.Delete(ev.EventId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
    
    [Fact]
public async Task GetById_ShouldReturnOk_WhenEventExists()
{
    // Arrange
    var ev = new Event 
    { 
        Title = "AI Summit", 
        Date = DateTime.UtcNow.AddDays(5), 
        Location = "Baku", 
        OrganizerId = 1 
    };
    _db.Events.Add(ev);
    await _db.SaveChangesAsync();

    var readDto = new EventReadDto { EventId = ev.EventId, Title = "AI Summit" };
    _mapper.Setup(m => m.Map<EventReadDto>(ev)).Returns(readDto);

    // Act
    var result = await _controller.GetById(ev.EventId);

    // Assert
    var actionResult = result as ActionResult<EventReadDto>;
    actionResult!.Result.Should().BeOfType<OkObjectResult>();
}

[Fact]
public async Task Update_ShouldReturnNoContent_WhenValidData()
{
    // Arrange
    var ev = new Event 
    { 
        Title = "Old Title", 
        Date = DateTime.UtcNow.AddDays(5), 
        Location = "Baku", 
        OrganizerId = 1 
    };
    _db.Organizers.Add(new Organizer 
    { 
        OrganizerId = 1, 
        Name = "Test Org", 
        Email = "org@test.com" 
    });
    _db.Events.Add(ev);
    await _db.SaveChangesAsync();

    var dto = new EventUpdateDto 
    { 
        Title = "New Title", 
        Date = DateTime.UtcNow.AddDays(10), 
        Location = "Baku", 
        OrganizerId = 1 
    };

    _updateValidator
        .Setup(v => v.ValidateAsync(dto, default))
        .ReturnsAsync(new ValidationResult());

    // Act
    var result = await _controller.Update(ev.EventId, dto);

    // Assert
    result.Should().BeOfType<NoContentResult>();
}

[Fact]
public async Task Update_ShouldReturnBadRequest_WhenValidationFails()
{
    // Arrange
    var dto = new EventUpdateDto { Title = "" };
    var validationResult = new ValidationResult(new[]
    {
        new ValidationFailure("Title", "Title is required.")
    });
    _updateValidator
        .Setup(v => v.ValidateAsync(dto, default))
        .ReturnsAsync(validationResult);

    // Act
    var result = await _controller.Update(1, dto);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>();
}

[Fact]
public async Task Update_ShouldReturnNotFound_WhenEventDoesNotExist()
{
    // Arrange
    var dto = new EventUpdateDto 
    { 
        Title = "New Title", 
        Date = DateTime.UtcNow.AddDays(10), 
        Location = "Baku", 
        OrganizerId = 1 
    };
    _updateValidator
        .Setup(v => v.ValidateAsync(dto, default))
        .ReturnsAsync(new ValidationResult());

    // Act
    var result = await _controller.Update(999, dto);

    // Assert
    result.Should().BeOfType<NotFoundResult>();
}

[Fact]
public async Task Delete_ShouldReturnNotFound_WhenEventDoesNotExist()
{
    // Act
    var result = await _controller.Delete(999);

    // Assert
    result.Should().BeOfType<NotFoundResult>();
}
}