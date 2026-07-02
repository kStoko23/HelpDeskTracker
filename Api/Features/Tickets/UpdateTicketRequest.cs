using Api.Entities;

namespace Api.Features.Tickets;

public record UpdateTicketRequest(
    string? Title,
    string? Description,
    TicketPriority? Priority,
    TicketStatus? Status,
    TicketCategory?  Category
    );