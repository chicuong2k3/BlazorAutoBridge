namespace BlazorAutoBridge.Sample.Client.Contracts;

public class CreateUpdateUserRequest
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
