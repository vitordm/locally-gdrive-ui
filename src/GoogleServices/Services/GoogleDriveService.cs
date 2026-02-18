using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using GoogleServices.Configurations;
using GoogleServices.Models.Requests;
using GoogleServices.Models.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoogleServices.Services;

public interface IGoogleDriveService
{
    Task<DriveUploadResult> UploadFileAsync(
        Stream streamFile,
        string fileName,
        string mimeType,
        IEnumerable<string>? parentFolderIds,
        CancellationToken cancellationToken
    );

    Task<DriveUploadResult> UploadFileAsync(DriveUploadRequest request, CancellationToken cancellationToken);

    Task<DriveFileListResult> ListFilesAsync(
        int pageSize,
        string? pageToken,
        string? query,
        string? orderBy,
        bool includeTrashed,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<DriveFileItem>> ListFilesByFolderAsync(
        string folderId,
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken
    );

    Task<MemoryStream> DownloadDriveFileAsync(string fileId, CancellationToken cancellationToken);
}

public class GoogleDriveService(IOptions<GoogleServicesSettings> settings, ILogger<GoogleDriveService> logger) : BaseGoogleServices(settings), IGoogleDriveService
{
    private const string DriveFileProjection = "id,name,mimeType,size,modifiedTime,parents,webViewLink";
    private readonly ILogger<GoogleDriveService> _logger = logger;

    public Task<DriveUploadResult> UploadFileAsync(
        Stream streamFile,
        string fileName,
        string mimeType,
        IEnumerable<string>? parentFolderIds,
        CancellationToken cancellationToken
    )
    {
        var request = new DriveUploadRequest(
            streamFile,
            fileName,
            mimeType,
            parentFolderIds?.ToArray()
        );
        return UploadFileAsync(request, cancellationToken);
    }

    public async Task<DriveUploadResult> UploadFileAsync(DriveUploadRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new ArgumentException("The file name is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.MimeType))
        {
            throw new ArgumentException("The file mime type is required.", nameof(request));
        }

        if (!request.Content.CanRead)
        {
            throw new ArgumentException("The stream must be readable.", nameof(request));
        }

        var service = await CreateDriveServiceAsync(cancellationToken);
        var metadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = request.FileName,
            Parents = request.ParentFolderIds?.ToList()
        };

        if (request.Content.CanSeek)
        {
            request.Content.Position = 0;
        }

        var upload = service.Files.Create(metadata, request.Content, request.MimeType);
        upload.Fields = DriveFileProjection;

        var uploadStatus = await upload.UploadAsync(cancellationToken);
        if (uploadStatus.Status is not UploadStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Google Drive upload failed with status '{uploadStatus.Status}' and exception '{uploadStatus.Exception?.Message}'."
            );
        }

        var uploadedFile = upload.ResponseBody
            ?? throw new InvalidOperationException("Google Drive did not return file metadata after upload.");

        _logger.LogInformation("Google Drive upload completed. FileId={FileId}, FileName={FileName}", uploadedFile.Id, uploadedFile.Name);

        return new DriveUploadResult(
            uploadedFile.Id ?? string.Empty,
            uploadedFile.Name ?? request.FileName,
            uploadedFile.MimeType ?? request.MimeType,
            uploadedFile.Size,
            uploadedFile.WebViewLink
        );
    }

    public async Task<DriveFileListResult> ListFilesAsync(
        int pageSize,
        string? pageToken,
        string? query,
        string? orderBy,
        bool includeTrashed,
        CancellationToken cancellationToken
    )
    {
        if (pageSize <= 0 || pageSize > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be between 1 and 1000.");
        }

        var service = await CreateDriveServiceAsync(cancellationToken);
        var request = service.Files.List();
        request.PageSize = pageSize;
        request.PageToken = pageToken;
        request.OrderBy = string.IsNullOrWhiteSpace(orderBy) ? "modifiedTime desc,name" : orderBy;
        request.Spaces = "drive";
        request.Fields = $"nextPageToken,files({DriveFileProjection})";

        var filters = new List<string>();
        if (!includeTrashed)
        {
            filters.Add("trashed = false");
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            filters.Add($"({query})");
        }

        if (filters.Count > 0)
        {
            request.Q = string.Join(" and ", filters);
        }

        var response = await request.ExecuteAsync(cancellationToken);
        var files = (response.Files ?? [])
            .Select(MapDriveFile)
            .ToList();

        return new DriveFileListResult(files, response.NextPageToken);
    }

    public async Task<IReadOnlyList<DriveFileItem>> ListFilesByFolderAsync(
        string folderId,
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(folderId))
        {
            throw new ArgumentException("Folder id is required.", nameof(folderId));
        }

        var escapedFolderId = folderId.Replace("'", "\\'", StringComparison.Ordinal);
        var query = $"'{escapedFolderId}' in parents";
        var listResult = await ListFilesAsync(
            pageSize,
            pageToken,
            query,
            orderBy: "folder,name",
            includeTrashed: false,
            cancellationToken
        );

        return listResult.Files;
    }

    public async Task<MemoryStream> DownloadDriveFileAsync(string fileId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException("File id is required.", nameof(fileId));
        }

        try
        {
            var service = await CreateDriveServiceAsync(cancellationToken);
            var request = service.Files.Get(fileId);
            var stream = new MemoryStream();

            request.MediaDownloader.ProgressChanged +=
                progress =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            _logger.LogDebug("Downloading Google Drive file {FileId}. BytesDownloaded={BytesDownloaded}", fileId, progress.BytesDownloaded);
                            break;
                        case DownloadStatus.Completed:
                            _logger.LogInformation("Google Drive download completed. FileId={FileId}", fileId);
                            break;
                        case DownloadStatus.Failed:
                            _logger.LogError("Google Drive download failed. FileId={FileId}, Error={Error}", fileId, progress.Exception?.Message);
                            break;
                    }
                };

            await request.DownloadAsync(stream, cancellationToken);
            stream.Position = 0;
            return stream;
        }
        catch (AggregateException ex)
        {
            _logger.LogError(ex, "Failed to resolve Google credentials while downloading file {FileId}.", fileId);
            throw;
        }
    }

    public Task<MemoryStream> DownloadDriveFile(string fileId, CancellationToken cancellationToken)
    {
        return DownloadDriveFileAsync(fileId, cancellationToken);
    }

    private async Task<DriveService> CreateDriveServiceAsync(CancellationToken cancellationToken)
    {
        GoogleCredential credential = await CreateCredentialByScopedAsync(DriveService.Scope.Drive, cancellationToken);

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "LocallyGDrive"
        });
    }

    private static DriveFileItem MapDriveFile(Google.Apis.Drive.v3.Data.File file)
    {
        return new DriveFileItem(
            file.Id ?? string.Empty,
            file.Name ?? string.Empty,
            file.MimeType ?? string.Empty,
            file.Size,
            file.ModifiedTime.HasValue ? new DateTimeOffset(file.ModifiedTime.Value) : null,
            file.Parents?.ToList()
        );
    }
}
