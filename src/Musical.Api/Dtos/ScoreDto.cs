namespace Musical.Api.Dtos;

public record ScoreDto(
    int Id,
    string Title,
    string? Composer,
    string? Description,
    string ImageFileName,
    DateTime UploadedAt,
    int AnnotationCount,
    int? FolderId,
    string? FolderName,
    string? FolderColor,
    string? FolderUserId,
    string? FolderUserDisplayName,
    string? FolderUserBio,
    string? FolderUserHeadshotFileName);

public record CreateScoreRequest(
    string Title,
    string? Composer,
    string? Description,
    int? FolderId);
