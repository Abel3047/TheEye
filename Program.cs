using TheEye.Application.Helpers;
using TheEye.Application.Interfaces;
using TheEye.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// DI
builder.Services.AddSingleton<IEyeSimulator, EyeSimulatorService>();
// register recorder (singleton) - optional: configure path via config
builder.Services.AddSingleton<IHistoryRecorder>(sp =>
    new HistoryRecorder(Path.Combine(AppContext.BaseDirectory, "logs", "eye_history.txt")));

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// serve wwwroot static files
builder.Services.AddDirectoryBrowser(); // optional if you want directory browsing

var app = builder.Build();

app.UseStaticFiles(); // serves wwwroot/index.html
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// simple health root
app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();
