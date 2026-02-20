namespace Musical.Web.Models;

public record ScoreSummary(
    int Id,
    string Title,
    string? Composer,
    string? Description,
    string ImageFileName,
    DateTime UploadedAt,
    int AnnotationCount);

public record AnnotationViewModel(
    int Id,
    int ScoreId,
    string AuthorName,
    string Content,
    double PositionX,
    double PositionY,
    DateTime CreatedAt);
