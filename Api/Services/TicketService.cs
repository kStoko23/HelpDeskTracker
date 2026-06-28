using Api.Common;
using Api.Data;
using Api.Features.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class TicketService
{
    private readonly HelpDeskDbContext _dbContext;

    public TicketService(HelpDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<List<TicketResponse>>> GetTicketsAsync(TicketQueryParameters queryParameters)
    {
        var query = _dbContext.Tickets.AsNoTracking().AsQueryable();

        if (queryParameters.Status != null) query = query.Where(x => x.Status == queryParameters.Status);
        if (queryParameters.Priority != null) query = query.Where(x => x.Priority == queryParameters.Priority);
        if (queryParameters.Category != null) query = query.Where(x => x.Category == queryParameters.Category);
        if (queryParameters.AssignedToId != null)
            query = query.Where(x => x.AssignedToId == queryParameters.AssignedToId);
        if (queryParameters.CreatedById != null) query = query.Where(x => x.CreatedById == queryParameters.CreatedById);

        var tickets = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .Select(x => new TicketResponse
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status.ToString(),
                Priority = x.Priority.ToString(),
                Category = x.Category.ToString(),
                CreatedAt = x.CreatedAt,
                CreatedById = x.CreatedById,
                AssignedToId = x.AssignedToId
            })
            .ToListAsync();

        return ServiceResult<List<TicketResponse>>.Success(tickets);
    }

    public async Task<ServiceResult<TicketDetailsResponse>> GetTicketByIdAsync(long id, long currentUserId,
        string currentUserRole)
    {
        var query = _dbContext.Tickets.AsNoTracking().Where(x => x.Id == id);

        if (currentUserRole == "Client") query = query.Where(x => x.CreatedById == currentUserId);

        var ticket = await query.Select(x => new TicketDetailsResponse
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status.ToString(),
                Priority = x.Priority.ToString(),
                Category = x.Category.ToString(),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                ClosedAt = x.ClosedAt,
                CreatedById = x.CreatedById,
                AssignedToId = x.AssignedToId,
                CreatedByUsername = x.CreatedBy.Username,
                AssignedToUsername = x.AssignedTo != null ? x.AssignedTo.Username : null,
            })
            .FirstOrDefaultAsync();

        if (ticket == null) return ServiceResult<TicketDetailsResponse>.NotFound("Ticket not found");

        return ServiceResult<TicketDetailsResponse>.Success(ticket);
    }

    public async Task<ServiceResult<List<TicketResponse>>> GetTicketsMineAsync(long userId)
    {
        var result = await _dbContext.Tickets
            .AsNoTracking()
            .Where(x => x.CreatedById == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TicketResponse
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status.ToString(),
                Priority = x.Priority.ToString(),
                Category = x.Category.ToString(),
                CreatedAt = x.CreatedAt,
                CreatedById = x.CreatedById,
                AssignedToId = x.AssignedToId,
            })
            .ToListAsync();
        
        return ServiceResult<List<TicketResponse>>.Success(result);
    }
}