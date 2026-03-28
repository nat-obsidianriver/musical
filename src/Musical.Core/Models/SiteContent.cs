namespace Musical.Core.Models;

public class SiteContent
{
    public int Id { get; set; }
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
