using FluentAssertions;
using WebApplication2.Dtos;
using WebApplication2.Validators;
using Xunit;

namespace WebApplication2.Tests.ControllerTests.EventTest;

public class EventCreateValidatorTests
{
    private readonly EventCreateValidator _validator = new();

    [Fact]
    public void Should_Pass_When_ValidData()
    {
        var dto = new EventCreateDto
        {
            Title = "AI Summit",
            Date = DateTime.UtcNow.AddDays(10),
            Location = "Baku",
            OrganizerId = 1
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_When_TitleIsEmpty()
    {
        var dto = new EventCreateDto
        {
            Title = "",
            Date = DateTime.UtcNow.AddDays(10),
            Location = "Baku",
            OrganizerId = 1
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Title" &&
            e.ErrorMessage == "Title is required.");
    }

    [Fact]
    public void Should_Fail_When_TitleExceedsMaxLength()
    {
        var dto = new EventCreateDto
        {
            Title = new string('A', 151),
            Date = DateTime.UtcNow.AddDays(10),
            Location = "Baku",
            OrganizerId = 1
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Title" &&
            e.ErrorMessage == "Title must not exceed 150 characters.");
    }

    [Fact]
    public void Should_Fail_When_DateIsInPast()
    {
        var dto = new EventCreateDto
        {
            Title = "AI Summit",
            Date = DateTime.UtcNow.AddDays(-1),
            Location = "Baku",
            OrganizerId = 1
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Date" &&
            e.ErrorMessage == "Event date must be in the future.");
    }

    [Fact]
    public void Should_Fail_When_LocationIsEmpty()
    {
        var dto = new EventCreateDto
        {
            Title = "AI Summit",
            Date = DateTime.UtcNow.AddDays(10),
            Location = "",
            OrganizerId = 1
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Location" &&
            e.ErrorMessage == "Location is required.");
    }

    [Fact]
    public void Should_Fail_When_LocationExceedsMaxLength()
    {
        var dto = new EventCreateDto
        {
            Title = "AI Summit",
            Date = DateTime.UtcNow.AddDays(10),
            Location = new string('A', 201),
            OrganizerId = 1
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Location" &&
            e.ErrorMessage == "Location must not exceed 200 characters.");
    }

    [Fact]
    public void Should_Fail_When_OrganizerIdIsZero()
    {
        var dto = new EventCreateDto
        {
            Title = "AI Summit",
            Date = DateTime.UtcNow.AddDays(10),
            Location = "Baku",
            OrganizerId = 0
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "OrganizerId" &&
            e.ErrorMessage == "A valid OrganizerId is required.");
    }

    [Fact]
    public void Should_Fail_When_DescriptionExceedsMaxLength()
    {
        var dto = new EventCreateDto
        {
            Title = "AI Summit",
            Description = new string('A', 501),
            Date = DateTime.UtcNow.AddDays(10),
            Location = "Baku",
            OrganizerId = 1
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Description" &&
            e.ErrorMessage == "Description must not exceed 500 characters.");
    }
}
