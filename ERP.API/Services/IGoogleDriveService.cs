using ERP.API.Models;

namespace ERP.API.Services
{
    public interface IGoogleDriveService
    {
    Task<DocumentUploadResponse> UploadFileAsync(IFormFile file, string? folderId = null, bool makePublic = false);
    Task<string> CreateFolderAsync(string folderName, string? parentFolderId = null);
    Task<bool> DeleteFileAsync(string fileId);
    Task<string> ShareFileAsync(string fileId, string email, string role = "reader");
    Task<string> MakeFilePublicAsync(string fileId);
    Task<FileInfoResponse?> GetFileInfoAsync(string fileId);
    Task<List<FileInfoResponse>> ListFilesAsync(string? folderId = null, int maxResults = 100);
    Task<bool> FileExistsAsync(string fileId);
    Task<bool> ValidateCredentialsAsync();
    Task<(System.IO.Stream? Stream, string? MimeType, string? FileName)> DownloadFileAsync(string fileId);
    }
}
