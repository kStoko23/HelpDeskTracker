using Api.Entities;

namespace Api.Features.Tickets;

public class TicketQueryParameters
{
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public TicketCategory? Category { get; set; }
    public long? AssignedToId { get; set; }
    public long? CreatedById { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 30;
}