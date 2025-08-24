using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorAutoBridge;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazorAutoBridge();

await builder.Build().RunAsync();
