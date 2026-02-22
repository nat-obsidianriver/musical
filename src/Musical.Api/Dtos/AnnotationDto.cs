namespace Musical.Api.Dtos;

public record AnnotationDto(
    int Id,
    int ScoreId,
    string AuthorName,
    string Content,
    double PositionX,
    double PositionY,
    double? PositionXEnd,
    double? PositionYEnd,
    string? AttachmentFileName,
    DateTime CreatedAt);

public class CreateAnnotationForm
{
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double? PositionXEnd { get; set; }
    public double? PositionYEnd { get; set; }
    public IFormFile? Attachment { get; set; }
}
