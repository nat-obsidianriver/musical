namespace Musical.Api.Dtos;

public record AnnotationDto(
    int Id,
    int ScoreId,
    string AuthorName,
    string Content,
    double PositionX,
    double PositionY,
    DateTime CreatedAt);

public record CreateAnnotationRequest(
    string AuthorName,
    string Content,
    double PositionX,
    double PositionY);
