namespace BlazorAutoBridge.Sample.Client.Contracts;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Username: {Username}\nEmail: {Email}";
    }
}
