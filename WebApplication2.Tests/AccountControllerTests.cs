using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using WebApplication2.Controllers;
using WebApplication2.Dtos.UserDtos;
using WebApplication2.Entities;
using WebApplication2.Service;
using Xunit;

namespace WebApplication2.Tests.ControllerTests;

public class AccountControllerTests
{
    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly Mock<RoleManager<IdentityRole>> _roleManager;
    private readonly Mock<IValidator<RegisterDto>> _registerValidator;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IEmailService> _emailService;
    private readonly Mock<IConfiguration> _config;
    private readonly JwtService _jwtService;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        // UserManager требует специального мока
        _userManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        // RoleManager требует специального мока
        _roleManager = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(),
            null, null, null, null);

        _registerValidator = new Mock<IValidator<RegisterDto>>();
        _mapper = new Mock<IMapper>();
        _emailService = new Mock<IEmailService>();
        _config = new Mock<IConfiguration>();
        _jwtService = new JwtService();

        _controller = new AccountController(
            _registerValidator.Object,
            _userManager.Object,
            _roleManager.Object,
            _config.Object,
            _jwtService,
            _mapper.Object,
            _emailService.Object);
    }

    // ─── Register Tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var dto = new RegisterDto { UserName = "", Email = "test@test.com", Password = "123456" };
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("UserName", "Username is required")
        });
        _registerValidator
            .Setup(v => v.Validate(dto))
            .Returns(validationResult);

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
    {
        // Arrange
        var dto = new RegisterDto { UserName = "existinguser", Email = "test@test.com", Password = "123456", FullName = "Test" };
        _registerValidator
            .Setup(v => v.Validate(dto))
            .Returns(new ValidationResult());
        _userManager
            .Setup(u => u.FindByNameAsync(dto.UserName))
            .ReturnsAsync(new AppUser { UserName = dto.UserName });

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Username already exists");
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenValidData()
    {
        // Arrange
        var dto = new RegisterDto { UserName = "newuser", Email = "test@test.com", Password = "123456", FullName = "Test" };
        var user = new AppUser { UserName = dto.UserName, Email = dto.Email };

        _registerValidator
            .Setup(v => v.Validate(dto))
            .Returns(new ValidationResult());
        _userManager
            .Setup(u => u.FindByNameAsync(dto.UserName))
            .ReturnsAsync((AppUser?)null);
        _mapper
            .Setup(m => m.Map<AppUser>(dto))
            .Returns(user);
        _userManager
            .Setup(u => u.CreateAsync(user, dto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(u => u.AddToRoleAsync(user, "Member"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(u => u.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("fake-token");
        _emailService
            .Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().Be("Registration successful. Please confirm your email.");
    }

    // ─── ForgotPassword Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var dto = new ForgotPasswordDto { Email = "notfound@test.com" };
        _userManager
            .Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _controller.ForgotPassword(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("User not found");
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var dto = new ForgotPasswordDto { Email = "test@test.com" };
        var user = new AppUser { Email = dto.Email };

        _userManager
            .Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
        _userManager
            .Setup(u => u.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("fake-reset-token");
        _emailService
            .Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ForgotPassword(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().Be("Password reset link sent to your email");
    }

    // ─── ResetPassword Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var dto = new ResetPasswordDto { Email = "notfound@test.com", Token = "token", NewPassword = "newpass" };
        _userManager
            .Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _controller.ResetPassword(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("User not found");
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnOk_WhenTokenIsValid()
    {
        // Arrange
        var user = new AppUser { Email = "test@test.com" };
        var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(
            System.Text.Encoding.UTF8.GetBytes("fake-token"));

        var dto = new ResetPasswordDto
        {
            Email = "test@test.com",
            Token = encodedToken,
            NewPassword = "newpassword123"
        };

        _userManager
            .Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
        _userManager
            .Setup(u => u.ResetPasswordAsync(user, "fake-token", dto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(u => u.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.ResetPassword(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().Be("Password reset successful");
    }

    // ─── Login Tests ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var dto = new LoginDto { UserName = "notexist", Password = "password" };
        _userManager
            .Setup(u => u.FindByNameAsync(dto.UserName))
            .ReturnsAsync((AppUser?)null);

        // Act
        Func<Task> act = async () => await _controller.Login(dto);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenEmailNotConfirmed()
    {
        // Arrange
        var dto = new LoginDto { UserName = "testuser", Password = "password" };
        var user = new AppUser { UserName = dto.UserName, EmailConfirmed = false };

        _userManager
            .Setup(u => u.FindByNameAsync(dto.UserName))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Login(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Please confirm your email first");
    }
}