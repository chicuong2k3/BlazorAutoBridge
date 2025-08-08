using System.Text;
using Microsoft.CodeAnalysis.Text;
using BlazorAutoBridge.Models;
using Microsoft.CodeAnalysis;

namespace BlazorAutoBridge.CodeGenerators;

internal static class ServerServiceGenerator
{
    public static void Generate(SourceProductionContext context, IReadOnlyList<ApiInterfaceInfo> apiInfos)
    {
        foreach (var api in apiInfos)
        {
            var serverServiceNamespace = api.GetServerServiceNamespace();
            var serverServiceName = api.GetServerServiceName();
            var interfaceNamespace = api.GetServiceInterfaceNamespace(false);
            var interfaceName = api.GetServiceInterfaceName();

            var methodsCode = string.Join("\n\n", api.Methods.Select(method =>
$@"        public async {method.ReturnType} {method.Name}({string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"))})
        {{
            return await _api.{method.Name}({string.Join(", ", method.Parameters.Select(p => p.Name))});
        }}"));

            var code = $@"
using System;
using System.Threading.Tasks;
using {api.Namespace};
using {interfaceNamespace};

namespace {serverServiceNamespace}
{{
    internal class {serverServiceName} : {interfaceName}
    {{
        private readonly {api.Name} _api;

        public {serverServiceName}({api.Name} api)
        {{
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }}

{methodsCode}
    }}
}}";

            context.AddSource($"{serverServiceName}.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }
}
