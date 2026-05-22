using System.Security.Claims;
using System.Text;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    IMapper mapper) : ControllerBase
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
        return Ok("Registration successful");
        
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await userManager.FindByNameAsync(loginDto.UserName);
        if (user is null)
            return BadRequest("Invalid username or password");
        var passwordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)           
            return BadRequest("Invalid username or password");
        
        var roles = await userManager.GetRolesAsync(user);
       
        return Ok(new{
            token = jwtService.GenerateToken(user, roles, config)
        });
    }

    [HttpPost("profile")]
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
}