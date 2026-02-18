namespace GoogleServices.Models.Results;

public sealed record DriveUploadResult(
    string Id,
    string Name,
    string MimeType,
    long? Size,
    string? WebViewLink
);
