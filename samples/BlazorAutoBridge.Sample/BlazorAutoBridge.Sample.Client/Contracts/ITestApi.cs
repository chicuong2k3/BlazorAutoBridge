using RestEase;

namespace BlazorAutoBridge.Sample.Client.Contracts;


[ApiService]
public interface ITestApi
{
    [Get("test")]
    Task<Response<IEnumerable<object>>> GetTest();
}