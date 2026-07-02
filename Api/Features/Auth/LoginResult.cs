namespace Api.Features.Auth;

public class LoginResult
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool MustChangePassword { get; set; }
}