using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using WebApplication2.Dtos.UserDtos;
using WebApplication2.Entities;
using WebApplication2.Helper;
using WebApplication2.Service;

namespace WebApplication2.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountController(
    IValidator<RegisterDto> registerValidator,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration config,
    JwtService jwtService,
    IMapper mapper,
    IEmailService emailService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 200)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 409)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var validationResult = registerValidator.Validate(registerDto);
        if (!validationResult.IsValid)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult(validationResult.Errors
                    .Select(e => e.ErrorMessage).ToArray()));

        var user = await userManager.FindByNameAsync(registerDto.UserName);
        if (user is not null)
            return Conflict(ResponseModelHelper<string>
                .ConflictResult("Username already exists"));

        user = mapper.Map<AppUser>(registerDto);
        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult(result.Errors.Select(e => e.Description).ToArray()));

        await userManager.AddToRoleAsync(user, "Member");

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmLink = $"http://localhost:5000/Account/confirm-email?userId={user.Id}&token={encodedToken}";

        await emailService.SendAsync(
            user.Email!,
            "Confirm your email",
            $"<p>Click <a href='{confirmLink}'>here</a> to confirm your email.</p>");

        return Ok(ResponseModelHelper<string>
            .SuccessResult("Registration successful. Please confirm your email."));
    }

    
    
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 200)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("User not found"));

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult(result.Errors.Select(e => e.Description).ToArray()));

        return Ok(ResponseModelHelper<string>
            .SuccessResult("Email confirmed successfully"));
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(ResponseModelHelper<object>), 200)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await userManager.FindByNameAsync(loginDto.UserName);

        if (user is null)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("Invalid username or password"));

        if (!user.EmailConfirmed)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("Please confirm your email first"));

        var passwordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("Invalid username or password"));

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtService.GenerateToken(user, roles, config);
        var refreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return Ok(ResponseModelHelper<object>.SuccessResult(new { accessToken, refreshToken }));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ResponseModelHelper<object>), 200)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(dto.AccessToken);
        var userId = jwtToken.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("Invalid token"));

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("User not found"));

        if (user.RefreshToken != dto.RefreshToken)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("Invalid refresh token"));

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("Refresh token expired"));

        var roles = await userManager.GetRolesAsync(user);
        var newAccessToken = jwtService.GenerateToken(user, roles, config);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return Ok(ResponseModelHelper<object>
            .SuccessResult(new { accessToken = newAccessToken, refreshToken = newRefreshToken }));
    }

    // [HttpGet("profile")]
    // [Authorize]
    // public IActionResult Profile()
    // {
    //     var userId = User.FindFirst(ClaimTypes.NameIdentifier);
    //     var userName = User.Identity?.Name;
    //     var fullName = User.FindFirst("FullName")?.Value;
    //     var role = User.Claims.Where(c=>c.Type == ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();
    //     return Ok(new
    //     {
    //        userId,
    //        userName,
    //        fullName,
    //        role
    //     });
    // }
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ResponseModelHelper<object>), 200)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 401)]
    public IActionResult Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.Identity?.Name;
        var fullName = User.FindFirst("FullName")?.Value;
        var role = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .FirstOrDefault();

        return Ok(ResponseModelHelper<object>
            .SuccessResult(new { userId, userName, fullName, role }));
    }

    // [HttpPost]
    // public async Task<IActionResult> CreateRole()
    // {
    //     await roleManager.CreateAsync(new IdentityRole("Member"));
    //     await roleManager.CreateAsync(new IdentityRole("Admin"));
    //     
    //     return Ok("Roles created");
    // }
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 200)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("User not found"));

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetLink = $"http://localhost:5000/Account/reset-password?email={dto.Email}&token={encodedToken}";

        await emailService.SendAsync(
            dto.Email,
            "Reset Password",
            $"<p>Click <a href='{resetLink}'>here</a> to reset your password.</p>");

        return Ok(ResponseModelHelper<string>
            .SuccessResult("Password reset link sent to your email"));
    }
    
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 200)]
    [ProducesResponseType(typeof(ResponseModelHelper<string>), 400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult("User not found"));

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(ResponseModelHelper<string>
                .BadRequestResult(result.Errors.Select(e => e.Description).ToArray()));

        await userManager.UpdateSecurityStampAsync(user);

        return Ok(ResponseModelHelper<string>
            .SuccessResult("Password reset successful"));
    }
}