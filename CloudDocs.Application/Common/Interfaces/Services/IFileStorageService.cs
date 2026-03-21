namespace CloudDocs.Application.Common.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<Stream?> GetFileAsync(string path, CancellationToken cancellationToken = default);
    bool FileExists(string path);
}