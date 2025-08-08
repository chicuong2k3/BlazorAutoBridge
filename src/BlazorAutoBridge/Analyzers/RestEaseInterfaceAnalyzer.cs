using BlazorAutoBridge.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorAutoBridge.Analyzers;

internal class RestEaseInterfaceAnalyzer
{
    public IReadOnlyList<ApiInterfaceInfo> Analyze(GeneratorSyntaxContext context, InterfaceDeclarationSyntax source)
    {
        if (source.Parent is not BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            return Array.Empty<ApiInterfaceInfo>();
        }

        var interfaceSymbol = context.SemanticModel.GetDeclaredSymbol(source);
        if (interfaceSymbol == null)
        {
            return Array.Empty<ApiInterfaceInfo>();
        }

        var attributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("BlazorAutoBridge.ApiServiceAttribute");
        if (attributeSymbol == null || !interfaceSymbol.GetAttributes()
            .Any(attr => attr != null && attr.AttributeClass != null && attr.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
        {
            return Array.Empty<ApiInterfaceInfo>();
        }

        var info = new ApiInterfaceInfo
        {
            Name = source.Identifier.Text,
            Namespace = namespaceDeclarationSyntax.Name.ToString(),
            Methods = new List<ApiMethodInfo>()
        };

        foreach (var method in source.Members.OfType<MethodDeclarationSyntax>())
        {
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            var httpMethodAttr = methodSymbol.GetAttributes()
                .FirstOrDefault(attr => attr != null && attr.AttributeClass != null
                                    && (attr.AttributeClass.Name.Contains("Get") ||
                                       attr.AttributeClass.Name.Contains("Post") ||
                                       attr.AttributeClass.Name.Contains("Put") ||
                                       attr.AttributeClass.Name.Contains("Delete")));

            if (httpMethodAttr == null)
            {
                continue;
            }

            var route = httpMethodAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? methodSymbol.Name;

            if (httpMethodAttr == null || httpMethodAttr.AttributeClass == null)
            {
                continue;
            }

            var methodInfo = new ApiMethodInfo
            {
                Name = methodSymbol.Name,
                ReturnType = methodSymbol.ReturnType.ToString(),
                HttpMethod = httpMethodAttr.AttributeClass.Name.Replace("Attribute", ""),
                Route = route,
                Parameters = new List<ApiMethodParameterInfo>()
            };

            foreach (var param in methodSymbol.Parameters)
            {
                var paramAttr = param.GetAttributes()
                    .FirstOrDefault(attr => attr != null && attr.AttributeClass != null);

                var restEaseAttribute = paramAttr?.AttributeClass?.Name?.Replace("Attribute", "") ?? string.Empty;

                methodInfo.Parameters.Add(new ApiMethodParameterInfo
                {
                    Name = param.Name,
                    Type = param.Type.ToString(),
                    RestEaseAttribute = restEaseAttribute
                });
            }

            info.Methods.Add(methodInfo);
        }

        return new[] { info };
    }
}