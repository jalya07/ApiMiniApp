using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;

namespace WebApplication2;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;

        builder.Services.AddControllers();

        // Correct Swagger setup
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

       
            app.UseSwagger();
            app.UseSwaggerUI();
            

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
        app.UseExceptionHandler(a => a.Run(async ctx =>
        {
            var feature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(new { 
                error = feature?.Error.Message, 
                detail = feature?.Error.StackTrace 
            });
        }));
        app.Run();
    }
}