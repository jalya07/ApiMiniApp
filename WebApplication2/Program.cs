using Microsoft.AspNetCore.Identity;
using WebApplication2;
using WebApplication2.Data;
using WebApplication2.Entities;

var builder = WebApplication.CreateBuilder(args);

// Вызываем ServiceRegistration — регистрирует всё (Identity, AutoMapper, Validators и т.д.)
builder.Services.AddServices(builder.Configuration);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Roles
    foreach (var role in new[] { "Member", "Admin" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    // Test user
    if (await userManager.FindByNameAsync("admin") == null)
    {
        var user = new AppUser { UserName = "admin", Email = "jalahasanli07@gmail.com", FullName = "Admin" };
        await userManager.CreateAsync(user, "password123");
        await userManager.AddToRoleAsync(user, "Admin");

        // UserEvents seed — после того как user создан
        if (!db.UserEvents.Any())
        {
            db.UserEvents.AddRange(
                new UserEvent { AppUserId = user.Id, EventId = 1 },
                new UserEvent { AppUserId = user.Id, EventId = 2 }
            );
            await db.SaveChangesAsync();
        }
    }
}

// app.UseExceptionHandler(a => a.Run(async ctx =>
// {
//     var feature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
//     ctx.Response.ContentType = "application/json";
//     await ctx.Response.WriteAsJsonAsync(new { 
//         error = feature?.Error.Message, 
//         inner = feature?.Error.InnerException?.Message, 
//         inner2 = feature?.Error.InnerException?.InnerException?.Message,
//         detail = feature?.Error.StackTrace 
//     });
// }));

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

app.Run();