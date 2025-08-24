using BlazorAutoBridge.Analyzers;
using BlazorAutoBridge.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace BlazorAutoBridge.CodeGenerators;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        GenerateHttpResponseAdapter(context);
        GenerateAllowAnyStatusCodeAttributes(context);

        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => IsTarget(node),
            transform: static (ctx, _) => GetSemanticSyntax(ctx)
        ).Where(x => x != null)
        .Collect()
        .Select((lists, _) => lists.SelectMany(l => l).ToList());


        var compilationProvider = context.CompilationProvider;
        var combined = provider.Combine(compilationProvider);

        var isClientProject = context.CompilationProvider
            .Select((compilation, _) => compilation.AssemblyName?.EndsWith(".Client") == true);

        var source = provider.Combine(compilationProvider).Combine(isClientProject);

        context.RegisterSourceOutput(source, static (ctx, tuple) =>
        {
            var ((apiInfos, compilation), isClient) = tuple;
            if (isClient)
            {
                ServiceInterfaceGenerator.Generate(ctx, apiInfos);
                ClientRestEaseInterfaceGenerator.Generate(ctx, apiInfos);
                ClientServiceGenerator.Generate(ctx, apiInfos);
                ClientDIGenerator.Generate(ctx, apiInfos);
            }
            else
            {
                ServerServiceGenerator.Generate(ctx, apiInfos);
                ForwarderGenerator.Generate(ctx, apiInfos);
                ServerDIGenerator.Generate(ctx, apiInfos);
            }
        });


        context.RegisterPostInitializationOutput(static (ctx) => PostInitializationOutput(ctx));
    }

    private static IReadOnlyList<ApiInterfaceInfo> GetSemanticSyntax(GeneratorSyntaxContext ctx)
    {
        var analyzer = new RestEaseInterfaceAnalyzer();
        var result = analyzer.Analyze(ctx, (InterfaceDeclarationSyntax)ctx.Node);
        return result;
    }

    private static bool IsTarget(SyntaxNode node)
    {
        return node is InterfaceDeclarationSyntax interfaceDeclarationSyntax;
    }

    private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext ctx)
    {
        GenerateApiServiceInterface(ctx);
    }

    private static void GenerateApiServiceInterface(IncrementalGeneratorPostInitializationContext ctx)
    {
        var code = $@"
using RestEase;

namespace BlazorAutoBridge
{{
    public interface IApiService
    {{
    }}
}}
";

        ctx.AddSource("IApiService.g.cs", SourceText.From(code, Encoding.UTF8));
    }


    private static void GenerateAllowAnyStatusCodeAttributes(IncrementalGeneratorInitializationContext context)
    {
        var symbolProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is InterfaceDeclarationSyntax,
            transform: static (ctx, _) =>
            {
                if (ctx.Node is InterfaceDeclarationSyntax ids)
                    return ctx.SemanticModel.GetDeclaredSymbol(ids) as INamedTypeSymbol;
                return null;
            }
        ).Where(x => x is not null)!;

        var combined = symbolProvider.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combined, static (spc, tuple) =>
        {
            var (symbol, compilation) = tuple;
            if (symbol is not INamedTypeSymbol iface) return;

            var apiServiceSymbol = compilation.GetTypeByMetadataName("BlazorAutoBridge.IApiService");
            if (apiServiceSymbol == null)
                return;

            if (!iface.AllInterfaces.Contains(apiServiceSymbol, SymbolEqualityComparer.Default))
                return;

            if (iface.GetAttributes().Any(a => a.AttributeClass?.Name == "AllowAnyStatusCodeAttribute"))
                return;

            var code = $@"
using RestEase;

namespace {iface.ContainingNamespace.ToDisplayString()}
{{
    [AllowAnyStatusCode]
    public partial interface {iface.Name} {{ }}
}}";

            spc.AddSource($"{iface.Name}.AllowAnyStatusCode.g.cs", SourceText.From(code, Encoding.UTF8));
        });
    }


    private static void GenerateHttpResponseAdapter(IncrementalGeneratorInitializationContext ctx)
    {
        var isClientProject = ctx.CompilationProvider
            .Select((compilation, _) => compilation.AssemblyName?.Contains(".Client") == true);

        ctx.RegisterSourceOutput(isClientProject, (ctx, isClient) =>
        {
            if (isClient) return;

            const string code = @"
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BlazorAutoBridge.HttpResponseAdapter
{
    public static class HttpResponseAdapterExtensions
    {
        public static async Task<IResult> ToIResultAsync(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return Results.StatusCode((int)response.StatusCode);

            try
            {
                var json = JsonSerializer.Deserialize<object>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return Results.Json(json, statusCode: (int)response.StatusCode);
            }
            catch
            {
                return Results.Content(content, ""text/plain"", statusCode: (int)response.StatusCode);
            }
        }
    }
}";

            ctx.AddSource("HttpResponseAdapterExtensions.g.cs", SourceText.From(code, Encoding.UTF8));
        });
    }
}
