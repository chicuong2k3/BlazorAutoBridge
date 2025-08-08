namespace BlazorAutoBridge.Models;

internal class ApiInterfaceInfo
{
    public string Namespace { get; set; } = default!;
    public string Name { get; set; } = default!;
    public List<ApiMethodInfo> Methods { get; set; } = new List<ApiMethodInfo>();

    public string GetServiceInterfaceNamespace(bool isClient)
    {
        var index = Namespace.LastIndexOf('.');
        return isClient ? $"{Namespace.Substring(0, index)}.Services" : $"{Namespace.Substring(0, index)}.Client.Services";
    }

    public string GetServiceInterfaceName()
    {
        return $"{Name.Replace("Api", "")}Service";
    }

    public string GetServerServiceNamespace()
    {
        var index = Namespace.LastIndexOf('.');
        return $"{Namespace.Substring(0, index)}.Services";
    }

    public string GetServerServiceName()
    {
        return $"{Name.Substring(1).Replace("Api", "")}Service";
    }

    public string GetClientServiceNamespace()
    {
        var index = Namespace.LastIndexOf('.');
        return $"{Namespace.Substring(0, index)}.Services";
    }

    public string GetClientServiceName()
    {
        return $"{Name.Substring(1).Replace("Api", "")}Service";
    }

    public string GetRestEaseClientNamespace()
    {
        var index = Namespace.LastIndexOf('.');
        return $"{Namespace.Substring(0, index)}.Client.Services";
    }

    public string GetRestEaseClientName()
    {
        return $"{Name.Replace("Api", "")}RestEaseClient";
    }
}
