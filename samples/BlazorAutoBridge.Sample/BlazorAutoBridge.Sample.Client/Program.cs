using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorAutoBridge.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazorAutoBridge((sp, client) =>
{
    client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}forwarders");
    client.Timeout = TimeSpan.FromSeconds(5);
});

await builder.Build().RunAsync();
