using FluentAssertions;
using WebApplication2.Dtos.OrganizerDto;
using WebApplication2.Validators;
using Xunit;

namespace WebApplication2.Tests.ControllerTests.OrganizerTest;

public class OrganizerCreateValidatorTests
{
    private readonly OrganizerCreateValidator _validator = new();

    [Fact]
    public void Should_Pass_When_ValidData()
    {
        var dto = new OrganizerCreateDto
        {
            Name = "TechConf Inc.",
            Email = "contact@techconf.com"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_When_NameIsEmpty()
    {
        var dto = new OrganizerCreateDto
        {
            Name = "",
            Email = "contact@techconf.com"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Name" &&
            e.ErrorMessage == "Name is required.");
    }

    [Fact]
    public void Should_Fail_When_NameExceedsMaxLength()
    {
        var dto = new OrganizerCreateDto
        {
            Name = new string('A', 101),
            Email = "contact@techconf.com"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Name" &&
            e.ErrorMessage == "Name must not exceed 100 characters.");
    }

    [Fact]
    public void Should_Fail_When_EmailIsEmpty()
    {
        var dto = new OrganizerCreateDto
        {
            Name = "TechConf Inc.",
            Email = ""
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Email" &&
            e.ErrorMessage == "Email is required.");
    }

    [Fact]
    public void Should_Fail_When_EmailIsInvalid()
    {
        var dto = new OrganizerCreateDto
        {
            Name = "TechConf Inc.",
            Email = "notanemail"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Email" &&
            e.ErrorMessage == "A valid email address is required.");
    }

    [Fact]
    public void Should_Fail_When_PhoneExceedsMaxLength()
    {
        var dto = new OrganizerCreateDto
        {
            Name = "TechConf Inc.",
            Email = "contact@techconf.com",
            Phone = new string('1', 21)
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Phone" &&
            e.ErrorMessage == "Phone must not exceed 20 characters.");
    }
}