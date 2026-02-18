namespace GoogleServices.Models.Results;

public sealed record DriveFileListResult(
    IReadOnlyList<DriveFileItem> Files,
    string? NextPageToken
);
