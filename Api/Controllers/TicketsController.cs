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
    public async Task<IActionResult> PostTicket()
    {
        throw new NotImplementedException();
    }

    [HttpPatch("{id:long}")]
    [Authorize]
    public async Task<IActionResult> PatchTicket(long id)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteTicket(long id)
    {
        throw new NotImplementedException();
    }
}