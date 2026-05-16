using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Entities;
using WebApplication2.Mapping;

namespace WebApplication2;

public static class ServiceRegistration
{
    public static void AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection")));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpContextAccessor();
        services.AddAutoMapper(opt=>
            opt.AddProfile(new MappingProfile(new HttpClientHandler())));
        services.AddIdentityCore<AppUser, IdentityRole>(opt =>
            {
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();;
        
    }
}