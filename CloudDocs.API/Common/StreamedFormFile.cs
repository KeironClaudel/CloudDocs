namespace CloudDocs.API.Common;

/// <summary>
/// Represents a multipart file section staged into temporary storage.
/// </summary>
public sealed class StreamedFormFile : IAsyncDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamedFormFile"/> class.
    /// </summary>
    /// <param name="tempFilePath">The temporary file path.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="length">The file length.</param>
    public StreamedFormFile(string tempFilePath, string fileName, string? contentType, long length)
    {
        TempFilePath = tempFilePath;
        FileName = fileName;
        ContentType = contentType;
        Length = length;
    }

    /// <summary>
    /// Gets the temporary file path.
    /// </summary>
    public string TempFilePath { get; }

    /// <summary>
    /// Gets the original file name.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the content type.
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    /// Gets the file length in bytes.
    /// </summary>
    public long Length { get; }

    /// <summary>
    /// Opens a read stream for the staged file.
    /// </summary>
    /// <returns>The file stream.</returns>
    public Stream OpenReadStream()
    {
        return new FileStream(
            TempFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (File.Exists(TempFilePath))
        {
            File.Delete(TempFilePath);
        }

        return ValueTask.CompletedTask;
    }
}
