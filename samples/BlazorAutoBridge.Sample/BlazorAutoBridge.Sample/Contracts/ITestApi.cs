using RestEase;

namespace BlazorAutoBridge.Sample.Contracts;


[ApiService]
public interface ITestApi
{
    [Get("test")]
    Task<Response<IEnumerable<object>>> GetTest();
}