using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Queries.GetUserLibrary;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly IMediator _mediator;

    public LibraryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(
        typeof(IReadOnlyList<UserLibraryGameDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var games = await _mediator.Send(
            new GetUserLibraryQuery(userId),
            cancellationToken);

        return Ok(games);
    }
}