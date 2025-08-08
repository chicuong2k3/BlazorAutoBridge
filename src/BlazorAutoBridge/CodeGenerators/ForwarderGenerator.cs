using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using BlazorAutoBridge.Models;

namespace BlazorAutoBridge.CodeGenerators;

internal static class ForwarderGenerator
{
    public static void Generate(SourceProductionContext context, IReadOnlyList<ApiInterfaceInfo> apiInterfaceInfos)
    {
        foreach (var api in apiInterfaceInfos)
        {
            var source = GenerateController(api);
            var fileName = $"{api.Name.Substring(1).Replace("Api", "")}Controller.g.cs";
            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateController(ApiInterfaceInfo api)
    {
        var sb = new StringBuilder();

        var controllerName = $"{api.Name.Substring(1).Replace("Api", "")}Controller";

        sb.AppendLine(@"
using Microsoft.AspNetCore.Mvc;
using BlazorAutoBridge.HttpResponseAdapter;
using System.Threading.Tasks;

namespace BlazorAutoBridge.Forwarders
{");

        sb.AppendLine($"\t[ApiController]");
        sb.AppendLine($"\t[Route(\"forwarders\")]");
        sb.AppendLine($"\tpublic class {controllerName} : ControllerBase");
        sb.AppendLine("\t{");
        sb.AppendLine($"\t\tprivate readonly {api.Namespace}.{api.Name} _api;");
        sb.AppendLine($"\t\tpublic {controllerName}({api.Namespace}.{api.Name} api)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\t_api = api;");
        sb.AppendLine("\t\t}");

        foreach (var method in api.Methods)
        {
            sb.AppendLine(GenerateControllerMethod(method));
        }

        sb.AppendLine("\t}");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateControllerMethod(ApiMethodInfo method)
    {
        var sb = new StringBuilder();

        var httpAttr = method.HttpMethod.ToUpperInvariant() switch
        {
            "GET" => $"[HttpGet(\"{method.Route}\")]",
            "POST" => $"[HttpPost(\"{method.Route}\")]",
            "PUT" => $"[HttpPut(\"{method.Route}\")]",
            "DELETE" => $"[HttpDelete(\"{method.Route}\")]",
            _ => $"[HttpGet(\"{method.Route}\")]"
        };

        sb.AppendLine($"\t\t{httpAttr}");

        var parameters = string.Join(", ", method.Parameters.Select(p => GenerateParameter(p)));
        var paramNames = string.Join(", ", method.Parameters.Select(p => p.Name));

        sb.AppendLine($"\t\tpublic async Task<IResult> {method.Name}({parameters})");
        sb.AppendLine("\t\t{");
        sb.AppendLine($"\t\t\tvar result = await _api.{method.Name}({paramNames});");
        sb.AppendLine("\t\t\treturn await result.ResponseMessage.ToIResultAsync();");
        sb.AppendLine("\t\t}");

        return sb.ToString();
    }

    private static string GenerateParameter(ApiMethodParameterInfo param)
    {
        var type = param.Type;
        var name = param.Name;

        switch (param.RestEaseAttribute.ToLowerInvariant())
        {
            case "query":
                return $"[FromQuery] {type} {name}";
            case "body":
                return $"[FromBody] {type} {name}";
            case "path":
                return $"{type} {name}";
            default:
                return $"{type} {name}";
        }
    }
}
