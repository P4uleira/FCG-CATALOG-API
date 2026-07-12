using FCG.Catalog.Application.DTOs;
using MediatR;

namespace FCG.Catalog.Application.Queries.GetUserLibrary;

public sealed record GetUserLibraryQuery(
    Guid UserId)
    : IRequest<IReadOnlyList<UserLibraryGameDto>>;