namespace Api.Features.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}