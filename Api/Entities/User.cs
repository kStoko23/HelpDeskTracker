namespace Api.Entities;

public class User
{
    public long Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Ticket> CreatedTickets { get; set; } = [];
    public ICollection<Ticket> AssignedTickets { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<TicketHistory> TicketHistories { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}