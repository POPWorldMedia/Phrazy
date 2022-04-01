using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Phrazy.Client;
using Phrazy.Client.Repositories;
using Phrazy.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IGameEngine, GameEngine>();
builder.Services.AddScoped<IPuzzleService, PuzzleService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IDeviceIDService, DeviceIDService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IGameStatePersistenceService, GameStatePersistenceService>();

builder.Services.AddScoped<IPuzzleRepo, PuzzleRepo>();

await builder.Build().RunAsync();
