using Api.Entities;

namespace Api.Features.Tickets;

public record CreateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    TicketCategory Category
    );
