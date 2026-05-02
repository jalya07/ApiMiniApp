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

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}