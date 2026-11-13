namespace FCG.Catalog.Application.DTOs;

public class GameSearchResultDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Genre { get; set; } = string.Empty;
    public double Score { get; set; }
}
