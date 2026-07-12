using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Queries.GetUserLibrary;

public sealed class GetUserLibraryQueryHandler
    : IRequestHandler<
        GetUserLibraryQuery,
        IReadOnlyList<UserLibraryGameDto>>
{
    private readonly IUserLibraryRepository _userLibraryRepository;

    private readonly ILogger<GetUserLibraryQueryHandler> _logger;

    public GetUserLibraryQueryHandler(
        IUserLibraryRepository userLibraryRepository,
        ILogger<GetUserLibraryQueryHandler> logger)
    {
        _userLibraryRepository = userLibraryRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserLibraryGameDto>> Handle(
        GetUserLibraryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Consultando biblioteca do usuário. UserId: {UserId}",
            request.UserId);

        var games =
            await _userLibraryRepository.GetGamesByUserIdAsync(
                request.UserId,
                cancellationToken);

        return games
            .Select(game => new UserLibraryGameDto(
                game.Id,
                game.Title,
                game.Description,
                game.Price,
                game.Genre,
                game.Active))
            .ToList();
    }
}