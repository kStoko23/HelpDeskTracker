using Api.Entities;

namespace Api.Features.Users;

public record CreateUserRequest(
    string Email,
    string Username,
    UserRole Role
);