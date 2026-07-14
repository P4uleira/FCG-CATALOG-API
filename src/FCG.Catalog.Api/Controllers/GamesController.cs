using System.Security.Claims;
using FCG.Catalog.Application.Commands.CreateGame;
using FCG.Catalog.Application.Commands.DeleteGame;
using FCG.Catalog.Application.Commands.PurchaseGame;
using FCG.Catalog.Application.Commands.UpdateGame;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Queries.GetGameById;
using FCG.Catalog.Application.Queries.GetGames;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    private readonly IMediator _mediator;

    public GamesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cadastra um novo jogo.
    /// Apenas administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateGameCommand command,
        CancellationToken cancellationToken)
    {
        var game = await _mediator.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = game.Id },
            game);
    }

    /// <summary>
    /// Retorna todos os jogos.
    /// Endpoint público.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var games = await _mediator.Send(
            new GetGamesQuery(),
            cancellationToken);

        return Ok(games);
    }

    /// <summary>
    /// Retorna um jogo pelo identificador.
    /// Endpoint público.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var game = await _mediator.Send(
            new GetGameByIdQuery
            {
                Id = id
            },
            cancellationToken);

        return Ok(game);
    }

    /// <summary>
    /// Atualiza um jogo.
    /// Apenas administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGameCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        var game = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(game);
    }

    /// <summary>
    /// Remove logicamente um jogo.
    /// Apenas administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteGameCommand
            {
                Id = id
            },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Inicia a compra de um jogo para o usuário autenticado.
    /// </summary>
    [Authorize]
    [HttpPost("{gameId:guid}/purchase")]
    [ProducesResponseType(
        typeof(PurchaseGameResponse),
        StatusCodes.Status202Accepted)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Purchase(
        Guid gameId,
        CancellationToken cancellationToken)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "O token JWT não contém um usuário válido."
            });
        }

        var command = new PurchaseGameCommand(
            gameId,
            userId);

        var response = await _mediator.Send(
            command,
            cancellationToken);

        return Accepted(response);
    }
}