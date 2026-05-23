using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApplication2.Data;
using WebApplication2.Entities;
using WebApplication2.Mapping;
using WebApplication2.Service;
using WebApplication2.Validators.UserValidator;

namespace WebApplication2;

public static class ServiceRegistration
{
  public static void AddServices(this IServiceCollection services, IConfiguration config)
{
    services.AddControllers();
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
    services.AddEndpointsApiExplorer();
    services.AddHttpContextAccessor();
    services.AddAutoMapper(opt =>
        opt.AddProfile(new MappingProfile(new HttpClientHandler())));
    services.AddValidatorsFromAssemblyContaining<RegisterCreateValidator>();
    services.AddScoped<IFileUploadService, FileUploadService>();
    services.AddScoped<JwtService>();

    // 1. Identity ПЕРВЫМ — он регистрирует свои схемы
    services.AddIdentity<AppUser, IdentityRole>(opt =>
    {
        opt.Password.RequireDigit = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // 2. Authentication ВТОРЫМ — перезаписывает схемы Identity на JWT
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

    // 3. Swagger с JWT (без изменений)
    services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}
}