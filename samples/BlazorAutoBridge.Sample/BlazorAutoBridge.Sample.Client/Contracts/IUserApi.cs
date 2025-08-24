using RestEase;

namespace BlazorAutoBridge.Sample.Client.Contracts;

public partial interface IUserApi : IApiService
{
    [Get("users")]
    Task<Response<IEnumerable<UserDto>>> GetAllUsers([Query] int Page, [Query] int PerPage);

    [Get("users/{id}")]
    Task<Response<UserDto>> GetSingle([Path] int id);

    [Post("users")]
    Task<Response<UserDto>> CreateUser([Body] CreateUpdateUserRequest request);

    [Delete("users/{id}")]
    Task<Response<object>> DeleteUser([Path] int id);

    [Put("users/{id}")]
    Task<Response<object>> UpdateUser([Path] int id, [Body] CreateUpdateUserRequest request);
}