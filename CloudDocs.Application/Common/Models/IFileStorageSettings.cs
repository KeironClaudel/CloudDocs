namespace CloudDocs.Application.Common.Models;

/// <summary>
/// Represents local file storage configuration settings.
/// </summary>
public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Gets or sets the root path.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the max file size in bytes.
    /// </summary>
    public long MaxFileSizeInBytes { get; set; }
}