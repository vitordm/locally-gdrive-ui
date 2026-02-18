namespace GoogleServices.Models.Results;

public sealed record DriveFileItem(
    string Id,
    string Name,
    string MimeType,
    long? Size,
    DateTimeOffset? ModifiedTime,
    IReadOnlyList<string>? Parents
);
