using BlazorAutoBridge.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace BlazorAutoBridge.CodeGenerators;

internal class ClientServiceGenerator
{
    public static void Generate(SourceProductionContext context, IReadOnlyList<ApiInterfaceInfo> apiInfos)
    {
        foreach (var api in apiInfos)
        {
            var source = GenerateService(api);
            context.AddSource($"{api.GetClientServiceName()}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateService(ApiInterfaceInfo api)
    {
        var methods = string.Join("\n\n", api.Methods.Select(m =>
        {
            var parameters = string.Join(", ", m.Parameters.Select(p => $"{p.Type} {p.Name}"));
            var argNames = string.Join(", ", m.Parameters.Select(p => p.Name));

            return
    @$"        public {m.ReturnType} {m.Name}({parameters})
        {{
            return _client.{m.Name}({argNames});
        }}";
        }));

        return
    @$"
using {api.GetServiceInterfaceNamespace(true)};
using {api.GetRestEaseClientNamespace()};

namespace {api.GetClientServiceNamespace()}
{{
    internal class {api.GetClientServiceName()} : {api.GetServiceInterfaceName()}
    {{
        private readonly {api.GetRestEaseClientName()} _client;

        public {api.GetClientServiceName()}({api.GetRestEaseClientName()} client)
        {{
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }}

{methods}
    }}
}}";
    }

}
