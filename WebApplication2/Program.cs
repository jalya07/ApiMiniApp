using WebApplication2;

var builder = WebApplication.CreateBuilder(args);

// Вызываем ServiceRegistration — регистрирует всё (Identity, AutoMapper, Validators и т.д.)
builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(a => a.Run(async ctx =>
{
    var feature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new {
        error = feature?.Error.Message,
        detail = feature?.Error.StackTrace
    });
}));

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication(); // ← обязательно ДО UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();