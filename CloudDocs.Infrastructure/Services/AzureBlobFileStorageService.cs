using Azure.Storage.Blobs;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace CloudDocs.Infrastructure.Services;

/// <summary>
/// Provides operations for Azure Blob Storage file handling.
/// </summary>
public class AzureBlobFileStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobFileStorageService"/> class.
    /// </summary>
    /// <param name="options">The Azure Blob Storage settings.</param>
    public AzureBlobFileStorageService(IOptions<AzureBlobSettings> options)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            throw new InvalidOperationException("Azure Blob connection string is not configured.");

        if (string.IsNullOrWhiteSpace(settings.ContainerName))
            throw new InvalidOperationException("Azure Blob container name is not configured.");

        var serviceClient = new BlobServiceClient(settings.ConnectionString);
        _containerClient = serviceClient.GetBlobContainerClient(settings.ContainerName);
        _containerClient.CreateIfNotExists();
    }

    /// <summary>
    /// Saves a file to Azure Blob Storage.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the blob name.</returns>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);

        if (fileStream.CanSeek)
            fileStream.Position = 0;

        await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken);

        return fileName;
    }

    /// <summary>
    /// Retrieves a file from Azure Blob Storage.
    /// </summary>
    /// <param name="path">The blob name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the stream when available; otherwise, null.</returns>
    public async Task<Stream?> GetFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);

        if (!await blobClient.ExistsAsync(cancellationToken))
            return null;

        return await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Determines whether the blob exists in Azure Blob Storage.
    /// </summary>
    /// <param name="path">The blob name.</param>
    /// <returns>true if the blob exists; otherwise, false.</returns>
    public bool FileExists(string path)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        return blobClient.Exists().Value;
    }
}
