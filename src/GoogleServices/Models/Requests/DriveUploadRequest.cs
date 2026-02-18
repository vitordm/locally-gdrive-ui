namespace GoogleServices.Models.Requests;

public sealed record DriveUploadRequest(
    Stream Content,
    string FileName,
    string MimeType,
    IReadOnlyCollection<string>? ParentFolderIds = null
);
