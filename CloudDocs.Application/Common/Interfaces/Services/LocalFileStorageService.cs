using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace CloudDocs.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<FileStorageSettings> options)
    {
        _settings = options.Value;
        _rootPath = Path.GetFullPath(_settings.RootPath);

        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, fileName);

        await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(output, cancellationToken);

        return fileName;
    }

    public async Task<Stream?> GetFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, path);

        if (!File.Exists(fullPath))
            return null;

        var memoryStream = new MemoryStream();
        await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public bool FileExists(string path)
    {
        var fullPath = Path.Combine(_rootPath, path);
        return File.Exists(fullPath);
    }
}