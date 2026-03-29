namespace CloudDocs.Application.Common.Interfaces.Services;

/// <summary>
/// Defines the contract for file storage operations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves the file.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string value.</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the file.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the stream when available; otherwise, null.</returns>
    Task<Stream?> GetFileAsync(string path, CancellationToken cancellationToken = default);
    /// <summary>
    /// Determines whether the file exists.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    bool FileExists(string path);
}