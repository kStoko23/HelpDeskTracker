using Api.Entities;

namespace Api.Features.Users;

public class UserQueryParameters
{ 
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public UserSortBy SortBy { get; set; } = UserSortBy.Username;
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}