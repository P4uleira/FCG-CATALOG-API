using FCG.Catalog.Application.Commands.CreateGame;
using FCG.Catalog.Application.Commands.DeleteGame;
using FCG.Catalog.Application.Commands.UpdateGame;
using FCG.Catalog.Application.Queries.GetGameById;
using FCG.Catalog.Application.Queries.GetGames;
using MediatR;
using FCG.Catalog.Application.Commands.PurchaseGame;
using FCG.Catalog.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IMediator _mediator;

    public GamesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateGameCommand command,
        CancellationToken cancellationToken)
    {
        var game = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var games = await _mediator.Send(new GetGamesQuery(), cancellationToken);
        return Ok(games);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var game = await _mediator.Send(new GetGameByIdQuery { Id = id }, cancellationToken);
        return Ok(game);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGameCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        var game = await _mediator.Send(command, cancellationToken);
        return Ok(game);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteGameCommand { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{gameId:guid}/purchase")]
    [ProducesResponseType(typeof(PurchaseGameResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Purchase(
    Guid gameId,
    [FromBody] PurchaseGameRequest request,
    CancellationToken cancellationToken)
    {
        var command = new PurchaseGameCommand(
            gameId,
            request.UserId);

        var response = await _mediator.Send(command, cancellationToken);

        return Accepted(response);
    }
}