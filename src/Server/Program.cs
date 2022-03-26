using Microsoft.AspNetCore.ResponseCompression;
using Phrazy.Server;
using Phrazy.Server.Repositories;
using Phrazy.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(sp.GetService<IConfiguration>()!["apiHost"])
});

var services = builder.Services;
services.AddTransient<IPuzzleService, PuzzleService>();

services.AddTransient<IResultRepository, ResultRepository>();

services.AddSingleton<IConfig, Config>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
