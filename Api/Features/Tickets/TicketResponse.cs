namespace Api.Features.Tickets;

public class TicketResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public string Category { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public long CreatedById { get; set; }
    public long? AssignedToId { get; set; }
}