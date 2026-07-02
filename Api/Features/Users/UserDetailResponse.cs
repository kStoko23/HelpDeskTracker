using Api.Entities;

namespace Api.Features.Users;

public record UserDetailResponse(
    long Id,
    string Username,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    int CreatedTicketsCount,
    int AssignedTicketsCount,
    int CommentsCount,
    DateTime? LastTicketCreatedAt
    );