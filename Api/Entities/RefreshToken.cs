namespace Api.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Revoked { get; set; }
}