using System.Text;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using BlazorAutoBridge.Models;

namespace BlazorAutoBridge.CodeGenerators;

internal static class ServerDIGenerator
{
    public static void Generate(SourceProductionContext context, IReadOnlyList<ApiInterfaceInfo> apiInfos)
    {
        var scopedRegistrations = string.Join("\n", apiInfos.Select(api =>
$@"            services.AddScoped<{api.GetServiceInterfaceNamespace(false)}.{api.GetServiceInterfaceName()}, {api.GetServerServiceNamespace()}.{api.GetServerServiceName()}>();"));

        var restEaseClients = string.Join("\n", apiInfos.Select(api =>
$@"            .UseWithRestEaseClient<{api.Namespace}.{api.Name}>()"));

        var code = $@"
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;

namespace BlazorAutoBridge.DependencyInjection
{{
    internal static class BlazorAutoBridgeExtensions
    {{
        public static IServiceCollection AddBlazorAutoBridge(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
        {{
{scopedRegistrations}

            services.AddHttpClient(""RestEaseClient"")
                .ConfigureHttpClient(configureClient)
{restEaseClients};

            services.AddControllers();

            return services;
        }}
    }}
}}";

        context.AddSource("BlazorAutoBridgeExtensions.Server.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}
