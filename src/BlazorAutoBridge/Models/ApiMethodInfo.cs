namespace BlazorAutoBridge.Models;

internal class ApiMethodInfo
{
    public string Name { get; set; } = default!;
    public string ReturnType { get; set; } = default!;
    public string HttpMethod { get; set; } = default!;
    public string Route { get; set; } = default!;
    public List<ApiMethodParameterInfo> Parameters { get; set; } = new List<ApiMethodParameterInfo>();
}