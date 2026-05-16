using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Entities;

public class AppUser: IdentityUser
{
    public string FullName { get; set; } = null!;
}