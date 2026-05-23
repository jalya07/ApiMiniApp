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
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        Console.WriteLine("Register endpoint hit");
        var validationResult = registerValidator.Validate(registerDto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        var user= await userManager.FindByNameAsync(registerDto.UserName);
        if (user is not null)        
            return BadRequest("Username already exists");
        user = mapper.Map<AppUser>(registerDto);
        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        await userManager.AddToRoleAsync(user, "Member");
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmLink = $"http://localhost:5000/Account/confirm-email?userId={user.Id}&token={encodedToken}";

        await emailService.SendAsync(
            user.Email!,
            "Confirm your email",
            $"<p>Click <a href='{confirmLink}'>here</a> to confirm your email.</p>"
        );

        return Ok("Registration successful. Please confirm your email.");
        
    }
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return BadRequest("User not found");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok("Email confirmed successfully");
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await userManager.FindByNameAsync(loginDto.UserName);
        if (!user.EmailConfirmed)
            return BadRequest("Please confirm your email first");
        if (user is null)
            return BadRequest("Invalid username or password");
        var passwordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)           
            return BadRequest("Invalid username or password");
        
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtService.GenerateToken(user, roles, config);
        var refreshToken = jwtService.GenerateRefreshToken();

        // сохраняем refresh token в базе
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return Ok(new { accessToken, refreshToken });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        // достаём userId из старого access token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(dto.AccessToken);
        var userId = jwtToken.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId is null) return BadRequest("Invalid token");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return BadRequest("User not found");

        // проверяем refresh token
        if (user.RefreshToken != dto.RefreshToken)
            return BadRequest("Invalid refresh token");
    
        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            return BadRequest("Refresh token expired");

        // генерируем новые токены
        var roles = await userManager.GetRolesAsync(user);
        var newAccessToken = jwtService.GenerateToken(user, roles, config);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
    }
    [HttpGet("profile")]
    [Authorize]
    public IActionResult Profile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier);
        var userName = User.Identity?.Name;
        var fullName = User.FindFirst("FullName")?.Value;
        var role = User.Claims.Where(c=>c.Type == ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();
        return Ok(new
        {
           userId,
           userName,
           fullName,
           role
        });
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
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null) return BadRequest("User not found");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    
        var resetLink = $"http://localhost:5000/Account/reset-password?email={dto.Email}&token={encodedToken}";
    
        await emailService.SendAsync(
            dto.Email,
            "Reset Password",
            $"<p>Click <a href='{resetLink}'>here</a> to reset your password.</p>"
        );

        return Ok("Password reset link sent to your email");
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null) return BadRequest("User not found");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
    
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Password reset successful");
    }
}