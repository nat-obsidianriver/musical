namespace Musical.Api.Dtos;

public record FolderDto(
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

public record CreateFolderRequest(string Name, string? Description, string? Color);

public record UpdateFolderRequest(string Name, string? Description, string? Color, bool IsMasked);
