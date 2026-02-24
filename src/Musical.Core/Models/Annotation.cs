namespace Musical.Core.Models;

public class Annotation
{
    public int Id { get; set; }
    public int ScoreId { get; set; }
    public Score Score { get; set; } = null!;

    public required string AuthorName { get; set; }
    public required string Content { get; set; }

    /// <summary>Identity user Id of the annotation author; null for legacy pre-auth annotations.</summary>
    public string? UserId { get; set; }

    /// <summary>Folder this annotation is filed under; null for legacy annotations.</summary>
    public int? FolderId { get; set; }
    public Folder? Folder { get; set; }

    /// <summary>Horizontal position as a percentage (0–100) of the image width.</summary>
    public double PositionX { get; set; }

    /// <summary>Vertical position as a percentage (0–100) of the image height.</summary>
    public double PositionY { get; set; }

    /// <summary>End X for range annotations (0–100); null for point annotations.</summary>
    public double? PositionXEnd { get; set; }

    /// <summary>End Y for range annotations (0–100); null for point annotations.</summary>
    public double? PositionYEnd { get; set; }

    /// <summary>Optional image attachment filename stored in uploads/.</summary>
    public string? AttachmentFileName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
