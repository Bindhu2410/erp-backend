using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using ERP.API.Models;
using System.Text.Json;

namespace ERP.API.Services
{
        public class GoogleDriveService : IGoogleDriveService, IDisposable
        {
            private readonly DriveService _driveService;
            private readonly ILogger<GoogleDriveService> _logger;
            private readonly string[] _scopes = { DriveService.Scope.Drive };

            public GoogleDriveService(IConfiguration configuration, ILogger<GoogleDriveService> logger)
            {
                _logger = logger;
                try
                {
                    GoogleCredential credential = null;
                    var credentialPath = configuration["GoogleDrive:CredentialPath"];
                    var credentialJson = configuration["GoogleDrive:CredentialJson"];

                    // Try CredentialPath first
                    if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
                    {
                        var fileInfo = new FileInfo(credentialPath);
                        if (fileInfo.Length == 0)
                        {
                            _logger.LogWarning($"Google Drive credential file exists but is empty: {credentialPath}. Please add your service account JSON to this file.");
                            credential = null;
                        }
                        else
                        {
                            credential = GoogleCredential.FromFile(credentialPath).CreateScoped(_scopes);
                        }
                    }
                    // Try CredentialJson - could be either raw JSON or a file path
                    else if (!string.IsNullOrEmpty(credentialJson))
                    {
                        // Check if it's a file path
                        if (File.Exists(credentialJson))
                        {
                            var fileInfo = new FileInfo(credentialJson);
                            if (fileInfo.Length == 0)
                            {
                                _logger.LogWarning($"Google Drive credential file exists but is empty: {credentialJson}. Please add your service account JSON to this file.");
                                credential = null;
                            }
                            else
                            {
                                credential = GoogleCredential.FromFile(credentialJson).CreateScoped(_scopes);
                            }
                        }
                        else if (credentialJson.TrimStart().StartsWith("{"))
                        {
                            // It's raw JSON
                            credential = GoogleCredential.FromJson(credentialJson).CreateScoped(_scopes);
                        }
                        else
                        {
                            _logger.LogWarning($"Google Drive credential not found. Path '{credentialJson}' doesn't exist and it's not valid JSON.");
                            credential = null;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Google Drive credentials not configured. CredentialPath and CredentialJson are not set.");
                        credential = null;
                    }

                    if (credential != null)
                    {
                        _driveService = new DriveService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = configuration["GoogleDrive:ApplicationName"] ?? "Drive API Application"
                        });
                        _logger.LogInformation("Google Drive service initialized successfully");
                    }
                    else
                    {
                        _logger.LogWarning("Google Drive service is not initialized. Google Drive features will not be available until credentials are configured.");
                        _driveService = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Google Drive service");
                    _driveService = null;
                }
            }

            public async Task<(System.IO.Stream? Stream, string? MimeType, string? FileName)> DownloadFileAsync(string fileId)
            {
                try
                {
                    if (_driveService == null)
                    {
                        _logger.LogError("Google Drive service is not initialized");
                        return (null, null, null);
                    }

                    var fileInfo = await GetFileInfoAsync(fileId);
                    if (fileInfo == null)
                        return (null, null, null);

                    var stream = new System.IO.MemoryStream();
                    if (fileInfo.MimeType == "application/vnd.google-apps.document")
                    {
                        // Google Docs: export as docx
                        var exportRequest = _driveService.Files.Export(fileId, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                        await exportRequest.DownloadAsync(stream);
                        stream.Position = 0;
                        return (stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileInfo.Name + ".docx");
                    }
                    else if (fileInfo.MimeType == "application/vnd.google-apps.spreadsheet")
                    {
                        // Google Sheets: export as xlsx
                        var exportRequest = _driveService.Files.Export(fileId, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        await exportRequest.DownloadAsync(stream);
                        stream.Position = 0;
                        return (stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileInfo.Name + ".xlsx");
                    }
                    else if (fileInfo.MimeType == "application/vnd.google-apps.presentation")
                    {
                        // Google Slides: export as pptx
                        var exportRequest = _driveService.Files.Export(fileId, "application/vnd.openxmlformats-officedocument.presentationml.presentation");
                        await exportRequest.DownloadAsync(stream);
                        stream.Position = 0;
                        return (stream, "application/vnd.openxmlformats-officedocument.presentationml.presentation", fileInfo.Name + ".pptx");
                    }
                    else
                    {
                        // Regular file: download
                        var request = _driveService.Files.Get(fileId);
                        await request.DownloadAsync(stream);
                        stream.Position = 0;
                        return (stream, fileInfo.MimeType, fileInfo.Name);
                    }
                }
                catch
                {
                    return (null, null, null);
                }
            }

        public async Task<DocumentUploadResponse> UploadFileAsync(IFormFile file, string? folderId = null, bool makePublic = false)
        {
            if (file == null || file.Length == 0)
                return new DocumentUploadResponse
                {
                    FileName = file?.FileName ?? "unknown",
                    Message = "File is required and cannot be empty",
                    IsSuccess = false
                };

            if (_driveService == null)
                return new DocumentUploadResponse
                {
                    FileName = file.FileName,
                    Message = "Google Drive service is not initialized. Please configure credentials.",
                    IsSuccess = false
                };

            try
            {


                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = file.FileName,
                    Parents = folderId != null ? new List<string> { folderId } : null
                };
                FilesResource.CreateMediaUpload request;
                using var stream = file.OpenReadStream();
                request = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
                request.Fields = "id, name, webViewLink, webContentLink, size, mimeType, parents";
                request.SupportsAllDrives = true;
                var progress = await request.UploadAsync();
                if (progress.Status == UploadStatus.Failed)
                {
                    _logger.LogError("Upload failed for file: {FileName}. Error: {Error}", file.FileName, progress.Exception?.Message);
                    throw new Exception($"Upload failed: {progress.Exception?.Message}");
                }
                var uploadedFile = request.ResponseBody;
                if (makePublic)
                {
                    await MakeFilePublicAsync(uploadedFile.Id);
                }
                _logger.LogInformation("File uploaded successfully: {FileName} with ID: {FileId}", file.FileName, uploadedFile.Id);
                return new DocumentUploadResponse
                {
                    FileId = uploadedFile.Id,
                    FileName = uploadedFile.Name,
                    FolderId = folderId,
                    WebViewLink = uploadedFile.WebViewLink,
                    WebContentLink = uploadedFile.WebContentLink,
                    Message = "File uploaded successfully",
                    IsSuccess = true,
                    FileSizeBytes = uploadedFile.Size ?? 0,
                    MimeType = uploadedFile.MimeType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                return new DocumentUploadResponse
                {
                    FileName = file.FileName,
                    Message = $"Upload failed: {ex.Message}",
                    IsSuccess = false
                };
            }
        }

        public async Task<string> CreateFolderAsync(string folderName, string? parentFolderId = null)
        {
            if (_driveService == null)
                throw new InvalidOperationException("Google Drive service is not initialized. Please configure credentials.");

            try
            {
                var folderMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = parentFolderId != null ? new List<string> { parentFolderId } : null
                };
                var request = _driveService.Files.Create(folderMetadata);
                request.Fields = "id, name, webViewLink";
                request.SupportsAllDrives = true;
                var folder = await request.ExecuteAsync();
                _logger.LogInformation("Folder created successfully: {FolderName} with ID: {FolderId}", folderName, folder.Id);
                return folder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder: {FolderName}", folderName);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            try
            {
                await _driveService.Files.Delete(fileId).ExecuteAsync();
                _logger.LogInformation("File deleted successfully: {FileId}", fileId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileId}", fileId);
                return false;
            }
        }

        public async Task<string> ShareFileAsync(string fileId, string email, string role = "reader")
        {
            try
            {
                var permission = new Google.Apis.Drive.v3.Data.Permission()
                {
                    Type = "user",
                    Role = role,
                    EmailAddress = email
                };
                await _driveService.Permissions.Create(permission, fileId).ExecuteAsync();
                var fileRequest = _driveService.Files.Get(fileId);
                fileRequest.Fields = "webViewLink";
                var file = await fileRequest.ExecuteAsync();
                _logger.LogInformation("File shared successfully: {FileId} with {Email} as {Role}", fileId, email, role);
                return file.WebViewLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing file: {FileId} with {Email}", fileId, email);
                throw;
            }
        }

        public async Task<string> MakeFilePublicAsync(string fileId)
        {
            try
            {
                var permission = new Google.Apis.Drive.v3.Data.Permission()
                {
                    Type = "anyone",
                    Role = "reader"
                };
                await _driveService.Permissions.Create(permission, fileId).ExecuteAsync();
                var fileRequest = _driveService.Files.Get(fileId);
                fileRequest.Fields = "webViewLink, webContentLink";
                var file = await fileRequest.ExecuteAsync();
                _logger.LogInformation("File made public: {FileId}", fileId);
                return file.WebViewLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making file public: {FileId}", fileId);
                throw;
            }
        }

        public async Task<FileInfoResponse?> GetFileInfoAsync(string fileId)
        {
            try
            {
                var request = _driveService.Files.Get(fileId);
                request.Fields = "id, name, mimeType, size, createdTime, modifiedTime, webViewLink, webContentLink, parents";
                request.SupportsAllDrives = true;
                var file = await request.ExecuteAsync();
                return new FileInfoResponse
                {
                    Id = file.Id,
                    Name = file.Name,
                    MimeType = file.MimeType,
                    Size = file.Size,
                    CreatedTime = file.CreatedTime,
                    ModifiedTime = file.ModifiedTime,
                    WebViewLink = file.WebViewLink,
                    WebContentLink = file.WebContentLink,
                    Parents = file.Parents?.ToList() ?? new List<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info: {FileId}", fileId);
                return null;
            }
        }

        public async Task<List<FileInfoResponse>> ListFilesAsync(string? folderId = null, int maxResults = 100)
        {
            if (_driveService == null)
            {
                _logger.LogError("Google Drive service is not initialized");
                return new List<FileInfoResponse>();
            }

            try
            {
                var request = _driveService.Files.List();
                request.PageSize = Math.Min(maxResults, 1000);
                request.Corpora = "allDrives";
                request.SupportsAllDrives = true;
                request.IncludeItemsFromAllDrives = true;
                
                if (!string.IsNullOrEmpty(folderId))
                {
                    request.Q = $"'{folderId}' in parents and trashed = false";
                }
                else
                {
                    request.Q = "trashed = false";
                }
                
                request.Fields = "nextPageToken, files(id, name, mimeType, size, createdTime, modifiedTime, webViewLink, parents, driveId)";
                var result = await request.ExecuteAsync();
                return result.Files?.Select(file => new FileInfoResponse
                {
                    Id = file.Id,
                    Name = file.Name,
                    MimeType = file.MimeType,
                    Size = file.Size,
                    CreatedTime = file.CreatedTime,
                    ModifiedTime = file.ModifiedTime,
                    WebViewLink = file.WebViewLink,
                    Parents = file.Parents?.ToList() ?? new List<string>()
                }).ToList() ?? new List<FileInfoResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files in folder: {FolderId}", folderId);
                return new List<FileInfoResponse>();
            }
        }

        public async Task<bool> FileExistsAsync(string fileId)
        {
            if (_driveService == null)
                return false;

            try
            {
                var request = _driveService.Files.Get(fileId);
                request.Fields = "id";
                request.SupportsAllDrives = true;
                await request.ExecuteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidateCredentialsAsync()
        {
            try
            {
                var request = _driveService.Files.List();
                request.PageSize = 1;
                request.Fields = "files(id)";
                var result = await request.ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Drive credential validation failed");
                return false;
            }
        }

        public void Dispose()
        {
            _driveService?.Dispose();
        }

    }
}
