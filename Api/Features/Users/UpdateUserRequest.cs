using Api.Entities;

namespace Api.Features.Users;

public record UpdateUserRequest(
        string? Email,
        string? Username,
        UserRole? Role
    );