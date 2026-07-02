using Api.Entities;

namespace Api.Features.Users;

public record UserResponse(
    long Id,
    string Username,
    string Email,
    UserRole Role,
    bool IsActive
    );