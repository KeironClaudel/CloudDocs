using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace CloudDocs.Infrastructure.Services;

/// <summary>
/// Provides operations for local file storage.
/// Supports nested relative paths for document organization.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly string _rootPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileStorageService"/> class.
    /// </summary>
    /// <param name="options">The file storage settings.</param>
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
    /// Saves a file to local storage using the provided relative path.
    /// Intermediate directories are created automatically if they do not exist.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">The relative file path to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the normalized relative path used to store the file.
    /// </returns>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));

        if (fileName.Contains(".."))
            throw new InvalidOperationException("Invalid file path.");

        var normalizedRelativePath = NormalizeRelativePath(fileName);
        var fullPath = Path.Combine(_rootPath, normalizedRelativePath);
        var directoryPath = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (fileStream.CanSeek)
            fileStream.Position = 0;

        await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(output, cancellationToken);

        return normalizedRelativePath;
    }

    /// <summary>
    /// Gets a file from local storage by relative path.
    /// </summary>
    /// <param name="path">The relative file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the file stream when available; otherwise, null.
    /// </returns>
    public async Task<Stream?> GetFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        if (path.Contains(".."))
            return null;

        var normalizedRelativePath = NormalizeRelativePath(path);
        var fullPath = Path.Combine(_rootPath, normalizedRelativePath);

        if (!File.Exists(fullPath))
            return null;

        return new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
    }

    /// <summary>
    /// Determines whether a file exists in local storage.
    /// </summary>
    /// <param name="path">The relative file path.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    public bool FileExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (path.Contains(".."))
            return false;

        var normalizedRelativePath = NormalizeRelativePath(path);
        var fullPath = Path.Combine(_rootPath, normalizedRelativePath);

        return File.Exists(fullPath);
    }

    private static string NormalizeRelativePath(string path)
    {
        return path
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }
}
