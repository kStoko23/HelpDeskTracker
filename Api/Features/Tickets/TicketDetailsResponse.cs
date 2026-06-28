namespace Api.Features.Tickets;

public class TicketDetailsResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public string Category { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public long CreatedById { get; set; }
    public string CreatedByUsername { get; set; } = null!;
    public long? AssignedToId { get; set; }
    public string? AssignedToUsername { get; set; }
}