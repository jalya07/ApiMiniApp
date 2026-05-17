using System.Security.Claims;
using System.Text;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApplication2.Dtos.UserDtos;
using WebApplication2.Entities;

namespace WebApplication2.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountController(
    IValidator<RegisterDto> registerValidator,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration config,
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
        var claims = new List<Claim>
        { 
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("FullName", user.FullName)
        }; 
        claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));
        var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken=new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);
        
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return Ok(new{token});
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