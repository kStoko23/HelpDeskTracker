using Api.Common;
using Api.Entities;
using Api.Features.Tickets;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets([FromQuery] TicketQueryParameters queryParameters)
    {
        var result = await _ticketService.GetTicketsAsync(queryParameters);

        if (!result.IsSuccess) return this.ToActionResult(result);
        
        return Ok(result.Data);
    }

    [HttpGet("{id:long}")]
    [Authorize]
    public async Task<IActionResult> GetTicket(long id)
    {
        var userId = User.GetUserId();
        var userRole = User.GetUserRole();
        var result = await _ticketService.GetTicketByIdAsync(id, userId, userRole);

        if (!result.IsSuccess) return this.ToActionResult(result);

        return Ok(result.Data);
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.GetUserId();
        var result =  await _ticketService.GetTicketsMineAsync(userId);
        
        if (!result.IsSuccess) return this.ToActionResult(result);

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostTicket(CreateTicketRequest request)
    {
        var userId = User.GetUserId();
        var result = await _ticketService.PostTicketAsync(request, userId);

        if (!result.IsSuccess) return this.ToActionResult(result);

        return CreatedAtAction(nameof(GetTicket), new { id = result.Data.Id }, result.Data);
    }

    [HttpPatch("{id:long}")]
    [Authorize]
    public async Task<IActionResult> UpdateTicket(long id, UpdateTicketRequest request)
    {
        var userId = User.GetUserId();
        var userRole = User.GetUserRole();
        var result = await _ticketService.UpdateTicketAsync(id, request, userId, userRole);
        
        if (!result.IsSuccess) return this.ToActionResult(result);
        
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "AgentOrAdmin")]
    public async Task<IActionResult> DeleteTicket(long id)
    {
        var userId = User.GetUserId();
        var userRole = User.GetUserRole();
        var result = await _ticketService.DeleteTicketAsync(id, userId, userRole);

        if (!result.IsSuccess) return this.ToActionResult(result);

        return NoContent();
    }
}