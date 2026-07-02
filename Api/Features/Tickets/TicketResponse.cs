namespace Api.Features.Tickets;

public record TicketResponse(
    long Id,
    string Title,
    string Status,
    string Priority,
    string Category,
    DateTime CreatedAt,
    long CreatedById,
    long? AssignedToId
);