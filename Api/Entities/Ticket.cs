namespace Api.Entities;

public class Ticket
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketCategory Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public long CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public long? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<TicketHistory> TicketHistories { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];
}