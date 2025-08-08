using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using BlazorAutoBridge.Models;

namespace BlazorAutoBridge.CodeGenerators;

internal static class ClientRestEaseInterfaceGenerator
{
    public static void Generate(SourceProductionContext ctx, IReadOnlyList<ApiInterfaceInfo> apiInfos)
    {
        foreach (var api in apiInfos)
        {
            var source = GenerateRestEaseInterface(api);
            ctx.AddSource($"{api.GetRestEaseClientName()}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateRestEaseInterface(ApiInterfaceInfo api)
    {
        var codeBuilder = new StringBuilder();

        codeBuilder.AppendLine("using RestEase;");
        codeBuilder.AppendLine("using System.Threading.Tasks;");
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"namespace {api.GetRestEaseClientNamespace()}");
        codeBuilder.AppendLine("{");
        codeBuilder.AppendLine($"\tpublic interface {api.GetRestEaseClientName()}");
        codeBuilder.AppendLine("\t{");

        foreach (var method in api.Methods)
        {
            codeBuilder.AppendLine(GenerateMethodSignature(method));
        }

        codeBuilder.AppendLine("\t}");
        codeBuilder.AppendLine("}");

        return codeBuilder.ToString();
    }

    private static string GenerateMethodSignature(ApiMethodInfo method)
    {
        var sb = new StringBuilder();

        var route = method.Route ?? "/";
        if (!route.StartsWith("/")) route = "/" + route;
        sb.AppendLine($"\t\t[{method.HttpMethod}(\"{route}\")]");

        var parameters = new List<string>();
        foreach (var p in method.Parameters)
        {
            var paramStr = $"[{p.RestEaseAttribute}] {p.Type} {p.Name}";
            parameters.Add(paramStr.Trim());
        }

        var paramList = string.Join(", ", parameters);
        sb.AppendLine($"\t\t{method.ReturnType} {method.Name}({paramList});");

        return sb.ToString();
    }

}
