namespace Api.Features.Tickets;

public record TicketDetailsResponse(
    long Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    long CreatedById,
    string CreatedByUsername,
    long? AssignedToId,
    string? AssignedToUsername
);