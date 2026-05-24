using FluentAssertions;
using WebApplication2.Dtos.TicketDto;
using WebApplication2.Validators;
using Xunit;

namespace WebApplication2.Tests.ControllerTests.TicketTest;

public class TicketCreateValidatorTests
{
    private readonly TicketCreateValidator _validator = new();

    [Fact]
    public void Should_Pass_When_ValidData()
    {
        var dto = new TicketCreateDto
        {
            Type = "VIP",
            Price = 299.99m,
            QuantityAvailable = 50
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_When_TypeIsEmpty()
    {
        var dto = new TicketCreateDto
        {
            Type = "",
            Price = 299.99m,
            QuantityAvailable = 50
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Type" &&
            e.ErrorMessage == "Ticket type is required.");
    }

    [Fact]
    public void Should_Fail_When_TypeExceedsMaxLength()
    {
        var dto = new TicketCreateDto
        {
            Type = new string('A', 51),
            Price = 299.99m,
            QuantityAvailable = 50
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Type" &&
            e.ErrorMessage == "Type must not exceed 50 characters.");
    }

    [Fact]
    public void Should_Fail_When_PriceIsZero()
    {
        var dto = new TicketCreateDto
        {
            Type = "VIP",
            Price = 0,
            QuantityAvailable = 50
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Price" &&
            e.ErrorMessage == "Price must be a positive value.");
    }

    [Fact]
    public void Should_Fail_When_PriceIsNegative()
    {
        var dto = new TicketCreateDto
        {
            Type = "VIP",
            Price = -10,
            QuantityAvailable = 50
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Price" &&
            e.ErrorMessage == "Price must be a positive value.");
    }

    [Fact]
    public void Should_Fail_When_QuantityIsNegative()
    {
        var dto = new TicketCreateDto
        {
            Type = "VIP",
            Price = 299.99m,
            QuantityAvailable = -1
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "QuantityAvailable" &&
            e.ErrorMessage == "Quantity must be non-negative.");
    }

    [Fact]
    public void Should_Pass_When_QuantityIsZero()
    {
        var dto = new TicketCreateDto
        {
            Type = "VIP",
            Price = 299.99m,
            QuantityAvailable = 0 // ← 0 разрешён
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}