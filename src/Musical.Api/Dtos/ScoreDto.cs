namespace Musical.Api.Dtos;

public record ScoreDto(
    int Id,
    string Title,
    string? Composer,
    string? Description,
    string ImageFileName,
    DateTime UploadedAt,
    int AnnotationCount);

public record CreateScoreRequest(
    string Title,
    string? Composer,
    string? Description);
