using System;
using GoogleServices.Configurations;
using System.Collections;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Options;
using Google.Apis.Services;
using Google.Apis.Download;

namespace GoogleServices.Services;

public interface IGoogleDriveService { }

public class GoogleDriveService(IOptions<GoogleServicesSettings> settings, ILogger<GoogleDriveService> logger) : BaseGoogleServices(settings), IGoogleDriveService
{
    private readonly ILogger<GoogleDriveService> _logger = logger
;

    public async Task UploadFileAsync(Stream streamFile, CancellationToken cancellationToken)
    {
        GoogleCredential credential = await CreateCredentialByScopedAsync(DriveService.Scope.Drive, cancellationToken);


        // Create Drive API service.
        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Drive API Snippets"
        });

        // Upload file photo.jpg on drive.
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = "photo.jpg"
        };
        FilesResource.CreateMediaUpload request;
        // Create a new file on drive.
        //using (var stream = new FileStream(filePath,
        //           FileMode.Open))
        using (streamFile)
        {
            // Create a new file, with metadata and stream.
            request = service.Files.Create(
                fileMetadata, streamFile, "image/jpeg");
            request.Fields = "id";
            request.Upload();
        }

        var file = request.ResponseBody;
        // Prints the uploaded file id.
        Console.WriteLine("File ID: " + file.Id);
        _logger.LogInformation("File Id = {FileId}",  file.Id);
    }

    public async Task<MemoryStream> DownloadDriveFile(string fileId, CancellationToken cancellationToken)
        {
            try
            {
                GoogleCredential credential = await CreateCredentialByScopedAsync(DriveService.Scope.Drive, cancellationToken);

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Drive API Snippets"
                });

                var request = service.Files.Get(fileId);
                var stream = new MemoryStream();

                // Add a handler which will be notified on progress changes.
                // It will notify on each chunk download and when the
                // download is completed or failed.
                request.MediaDownloader.ProgressChanged +=
                    progress =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Downloading:
                            {
                                Console.WriteLine(progress.BytesDownloaded);
                                break;
                            }
                            case DownloadStatus.Completed:
                            {
                                Console.WriteLine("Download complete.");
                                break;
                            }
                            case DownloadStatus.Failed:
                            {
                                Console.WriteLine("Download failed.");
                                break;
                            }
                        }
                    };
                request.Download(stream);

                return stream;
            }
            catch (Exception e)
            {
                // TODO(developer) - handle error appropriately
                if (e is AggregateException)
                {
                    Console.WriteLine("Credential Not found");
                }
                else
                {
                    throw;
                }
            }
            return null;
        }
}
