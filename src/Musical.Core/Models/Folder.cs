namespace Musical.Core.Models;

public class Folder
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string Color { get; set; } = "#87CEEB";
    public bool IsMasked { get; set; }

    /// <summary>IdentityUser Id (string) of the owning user.</summary>
    public required string UserId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;

    public List<Score> Scores { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
