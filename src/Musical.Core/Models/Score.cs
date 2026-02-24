namespace Musical.Core.Models;

public class Score
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Composer { get; set; }
    public string? Description { get; set; }
    public required string ImageFileName { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int? FolderId { get; set; }
    public Folder? Folder { get; set; }

    public ICollection<Annotation> Annotations { get; set; } = [];
}
