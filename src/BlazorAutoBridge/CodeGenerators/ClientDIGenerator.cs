using System.Text;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using BlazorAutoBridge.Models;

namespace BlazorAutoBridge.CodeGenerators;

internal static class ClientDIGenerator
{
    public static void Generate(SourceProductionContext context, IReadOnlyList<ApiInterfaceInfo> apiInfos)
    {
        var scopedRegistrations = string.Join("\n", apiInfos.Select(api =>
$@"            services.AddScoped<{api.GetServiceInterfaceNamespace(true)}.{api.GetServiceInterfaceName()}, {api.GetClientServiceNamespace()}.{api.GetClientServiceName()}>();"));

        var restEaseClients = string.Join("\n", apiInfos.Select(api =>
$@"            .UseWithRestEaseClient<{api.GetRestEaseClientNamespace()}.{api.GetRestEaseClientName()}>()"));

        var code = $@"
using System;
using System.Net.Http;
using RestEase.HttpClientFactory;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoBridge
{{
    internal static class BlazorAutoBridgeExtensions
    {{
        public static IServiceCollection AddBlazorAutoBridge(this IServiceCollection services)
        {{
{scopedRegistrations}

            services.AddHttpClient(""RestEaseClient"")
                .ConfigureHttpClient((sp, client) =>
{{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    client.BaseAddress = new Uri($""{{navigationManager.BaseUri}}forwarders"");
}})
{restEaseClients};

            return services;
        }}
    }}
}}";

        context.AddSource($"BlazorAutoBridgeExtensions.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}
