using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace CloudDocs.Infrastructure.Services;

/// <summary>
/// Provides operations for local file storage.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly string _rootPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileStorageService"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    public LocalFileStorageService(IOptions<FileStorageSettings> options)
    {
        _settings = options.Value;
        _rootPath = Path.GetFullPath(_settings.RootPath);

        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
        }
    }

    /// <summary>
    /// Saves the file.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string value.</returns>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, fileName);

        await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(output, cancellationToken);

        return fileName;
    }

    /// <summary>
    /// Gets the file.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the stream when available; otherwise, null.</returns>
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

    /// <summary>
    /// Determines whether the file exists.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool FileExists(string path)
    {
        var fullPath = Path.Combine(_rootPath, path);
        return File.Exists(fullPath);
    }
}