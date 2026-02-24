namespace Musical.Web.Models;

public record AuthResponse(
    string Token,
    string UserId,
    string DisplayName,
    string Email,
    string Role,
    DateTime Expiry);


public record ScoreSummary(
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

public record FolderSummary(
    int Id,
    string Name,
    string? Description,
    string Color,
    bool IsMasked,
    string UserId,
    string UserDisplayName,
    string? UserBio,
    string? UserHeadshotFileName,
    int ScoreCount,
    DateTime CreatedAt);

public record AnnotationViewModel(
    int Id,
    int ScoreId,
    string AuthorName,
    string Content,
    double PositionX,
    double PositionY,
    double? PositionXEnd,
    double? PositionYEnd,
    string? AttachmentFileName,
    DateTime CreatedAt,
    string? UserId,
    int? FolderId,
    string? FolderName,
    string? FolderColor);

public record AnnotationFolderGroup(
    int? FolderId,
    string FolderName,
    string FolderColor,
    List<AnnotationViewModel> Annotations);

public record AnnotationUserGroup(
    string AuthorName,
    string? UserId,
    List<AnnotationFolderGroup> Folders);
