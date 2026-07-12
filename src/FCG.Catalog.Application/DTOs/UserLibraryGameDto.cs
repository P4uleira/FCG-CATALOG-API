namespace FCG.Catalog.Application.DTOs;

public sealed record UserLibraryGameDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    string Genre,
    bool Active);