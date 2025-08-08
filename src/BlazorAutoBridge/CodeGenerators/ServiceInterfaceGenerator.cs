using BlazorAutoBridge.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace BlazorAutoBridge.CodeGenerators;

internal static class ServiceInterfaceGenerator
{
    public static void Generate(SourceProductionContext context, IReadOnlyList<ApiInterfaceInfo> apiInfos)
    {
        foreach (var api in apiInfos)
        {
            var interfaceNamespace = api.GetServiceInterfaceNamespace(true);
            var interfaceName = api.GetServiceInterfaceName();

            var methodSignatures = string.Join("\n", api.Methods.Select(m =>
                $"\t\t{m.ReturnType} {m.Name}({string.Join(", ", m.Parameters.Select(p => $"{p.Type} {p.Name}"))});"
            ));

            var code = $@"
using System.Threading.Tasks;

namespace {interfaceNamespace}
{{
    public interface {interfaceName}
    {{
{methodSignatures}
    }}
}}";

            context.AddSource($"{interfaceName}.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }
}
