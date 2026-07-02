using Api.Common;
using Api.Data;
using Api.Entities;
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
        
        var page = Math.Max(queryParameters.Page, 1);
        var pageSize = Math.Clamp(queryParameters.PageSize, 1, 100);
        
        var tickets = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TicketResponse(x.Id, x.Title, x.Status.ToString(), x.Priority.ToString(),
                x.Category.ToString(), x.CreatedAt, x.CreatedById, x.AssignedToId))
            .ToListAsync();

        return ServiceResult<List<TicketResponse>>.Success(tickets);
    }

    public async Task<ServiceResult<TicketDetailsResponse>> GetTicketByIdAsync(long id, long currentUserId,
        string currentUserRole)
    {
        var query = _dbContext.Tickets.AsNoTracking().Where(x => x.Id == id);

        if (currentUserRole == "Client") query = query.Where(x => x.CreatedById == currentUserId);

        var ticket = await query.Select(x => new TicketDetailsResponse(x.Id, x.Title, x.Description,
                x.Status.ToString(), x.Priority.ToString(), x.Category.ToString(), x.CreatedAt, x.UpdatedAt, x.ClosedAt,
                x.CreatedById, x.CreatedBy.Username, x.AssignedToId,
                x.AssignedTo != null ? x.AssignedTo.Username : null))
            .FirstOrDefaultAsync();

        if (ticket == null) return ServiceResult<TicketDetailsResponse>.NotFound("Ticket not found");

        return ServiceResult<TicketDetailsResponse>.Success(ticket);
    }

    public async Task<ServiceResult<List<TicketResponse>>> GetTicketsMineAsync(long userId)
    {
        var result = await _dbContext.Tickets.AsNoTracking()
            .Where(x => x.CreatedById == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TicketResponse(x.Id, x.Title, x.Status.ToString(), x.Priority.ToString(),
                x.Category.ToString(), x.CreatedAt, x.CreatedById, x.AssignedToId))
            .ToListAsync();

        return ServiceResult<List<TicketResponse>>.Success(result);
    }

    public async Task<ServiceResult<TicketResponse>> PostTicketAsync(CreateTicketRequest request, long currentUserId)
    {
        //TODO: add validation
        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = TicketStatus.Open,
            Category = request.Category,
            CreatedAt = DateTime.UtcNow,
            CreatedById = currentUserId,
        };

        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync();

        var response = new TicketResponse(ticket.Id, ticket.Title, ticket.Status.ToString(), ticket.Priority.ToString(),
            ticket.Category.ToString(), ticket.CreatedAt, ticket.CreatedById, ticket.AssignedToId);

        return ServiceResult<TicketResponse>.Success(response);
    }

    public async Task<ServiceResult> UpdateTicketAsync(long id, UpdateTicketRequest request, long currentUserId,
        string currentUserRole)
    {
        // TODO: add validation
        var ticket = await _dbContext.Tickets.FindAsync(id);

        if (ticket == null) return ServiceResult.NotFound("Ticket not found");
        if (currentUserRole != "Agent" && currentUserRole != "Admin")
            return ServiceResult.Forbidden("You do not have permission to update tickets");
        if (currentUserRole == "Agent" && ticket.CreatedById != currentUserId)
            return ServiceResult.Forbidden("You do not have permission to update this ticket");

        if (request.Title != null) ticket.Title = request.Title.Trim();
        if (request.Description != null) ticket.Description = request.Description.Trim();
        if (request.Priority.HasValue) ticket.Priority = request.Priority.Value;
        if (request.Status.HasValue) ticket.Status = request.Status.Value;
        if (request.Category.HasValue) ticket.Category = request.Category.Value;

        ticket.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteTicketAsync(long id, long currentUserId, string currentUserRole)
    {
        var ticket = await _dbContext.Tickets.FindAsync(id);

        if (ticket == null) return ServiceResult.NotFound("Ticket not found");

        if (currentUserRole != "Admin" && currentUserRole != "Agent")
            return ServiceResult.Forbidden("You do not have permission to delete tickets");

        if (currentUserRole == "Agent" && ticket.CreatedById != currentUserId)
            return ServiceResult.Forbidden("You do not have permission to delete this ticket");

        _dbContext.Tickets.Remove(ticket);
        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }
}