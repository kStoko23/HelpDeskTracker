using Api.Entities;

namespace Api.Features.Users;

public record CreateUserResponse(
    long Id,
    string Email,
    string Username,
    UserRole Role,
    string TemporaryPassword,
    DateTime TemporaryPasswordExpiresAt
);