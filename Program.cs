using TheEye.Application.Interfaces;
using TheEye.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IEyeSimulator, EyeSimulatorService>();
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// simple health root
app.MapGet("/", () => Results.Ok(new { status = "Eye Almanac API (with controller) running" }));

app.Run();