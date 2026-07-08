namespace FCG.Catalog.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Genre { get; private set; } = string.Empty;
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Game() { }

    public Game(string title, string description, decimal price, string genre)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Price = price;
        Genre = genre;
        Active = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string description, decimal price, string genre)
    {
        Title = title;
        Description = description;
        Price = price;
        Genre = genre;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePartial(
    string? title,
    string? description,
    decimal? price,
    string? genre,
    bool? active)
    {
        if (!string.IsNullOrWhiteSpace(title))
            Title = title;

        if (description is not null)
            Description = description;

        if (price.HasValue)
            Price = price.Value;

        if (!string.IsNullOrWhiteSpace(genre))
            Genre = genre;

        if (active.HasValue)
            Active = active.Value;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }
}