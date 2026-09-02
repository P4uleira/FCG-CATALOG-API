using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Queries.GetPurchaseHistory;
using FCG.Catalog.Application.Queries.GetUserLibrary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Retorna o historico de compras (aprovadas, rejeitadas e duplicadas)
    /// registrado no MongoDB para o usuario informado.
    /// </summary>
    [Authorize]
    [HttpGet("{userId:guid}/history")]
    [ProducesResponseType(
        typeof(IReadOnlyList<PurchaseHistoryDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var history = await _mediator.Send(
            new GetPurchaseHistoryQuery(userId),
            cancellationToken);

        return Ok(history);
    }
}