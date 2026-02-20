namespace Musical.Core.Models;

public class Annotation
{
    public int Id { get; set; }
    public int ScoreId { get; set; }
    public Score Score { get; set; } = null!;

    public required string AuthorName { get; set; }
    public required string Content { get; set; }

    /// <summary>Horizontal position as a percentage (0–100) of the image width.</summary>
    public double PositionX { get; set; }

    /// <summary>Vertical position as a percentage (0–100) of the image height.</summary>
    public double PositionY { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
