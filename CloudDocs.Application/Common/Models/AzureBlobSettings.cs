namespace CloudDocs.Application.Common.Models;

/// <summary>
/// Represents Azure Blob Storage configuration settings.
/// </summary>
public class AzureBlobSettings
{
    public const string SectionName = "AzureBlob";

    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}
