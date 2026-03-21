namespace CloudDocs.Application.Common.Models;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = string.Empty;
    public long MaxFileSizeInBytes { get; set; }
}