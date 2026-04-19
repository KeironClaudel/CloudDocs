using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Globalization;
using System.Text;

namespace CloudDocs.API.Common;

/// <summary>
/// Reads multipart form-data requests without buffering files into memory.
/// </summary>
public static class MultipartRequestReader
{
    private const int DefaultBufferSize = 64 * 1024;

    /// <summary>
    /// Reads a multipart form-data request and stages its file content into a temporary file.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed multipart request.</returns>
    public static async Task<MultipartFormDataRequest> ReadAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ContentType) ||
            !request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Content type must be multipart/form-data.");
        }

        request.EnableBuffering();

        if (request.Body.CanSeek)
        {
            request.Body.Position = 0;
        }

        var boundary = GetBoundary(request.ContentType);
        var reader = new MultipartReader(boundary, request.Body);
        var result = new MultipartFormDataRequest();

        try
        {
            MultipartSection? section;

            while ((section = await reader.ReadNextSectionAsync(cancellationToken)) is not null)
            {
                if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                {
                    continue;
                }

                if (HasFileContentDisposition(contentDisposition))
                {
                    if (result.File is not null)
                    {
                        throw new InvalidOperationException("Only one file per request is supported.");
                    }

                    result.File = await StageFileAsync(section, contentDisposition, cancellationToken);
                    continue;
                }

                if (HasFormDataContentDisposition(contentDisposition))
                {
                    var fieldName = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;

                    if (string.IsNullOrWhiteSpace(fieldName))
                    {
                        continue;
                    }

                    using var streamReader = new StreamReader(
                        section.Body,
                        encoding: GetEncoding(section) ?? Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: true,
                        leaveOpen: true);

                    var value = await streamReader.ReadToEndAsync(cancellationToken);
                    result.AddField(fieldName, value);
                }
            }

            return result;
        }
        catch
        {
            await result.DisposeAsync();
            throw;
        }
        finally
        {
            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }
        }
    }

    private static async Task<StreamedFormFile> StageFileAsync(
        MultipartSection section,
        ContentDispositionHeaderValue contentDisposition,
        CancellationToken cancellationToken)
    {
        var originalFileName =
            HeaderUtilities.RemoveQuotes(contentDisposition.FileNameStar).Value ??
            HeaderUtilities.RemoveQuotes(contentDisposition.FileName).Value;

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new InvalidOperationException("Uploaded file name is missing.");
        }

        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.upload");

        await using (var output = new FileStream(
            tempFilePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: DefaultBufferSize,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await section.Body.CopyToAsync(output, cancellationToken);
        }

        var fileInfo = new FileInfo(tempFilePath);

        return new StreamedFormFile(
            tempFilePath,
            Path.GetFileName(originalFileName),
            section.ContentType,
            fileInfo.Length);
    }

    private static string GetBoundary(string contentType)
    {
        var mediaType = MediaTypeHeaderValue.Parse(contentType);
        var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidOperationException("Missing multipart boundary.");
        }

        return boundary;
    }

    private static Encoding? GetEncoding(MultipartSection section)
    {
        if (MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType) &&
            !string.IsNullOrWhiteSpace(mediaType.Charset.Value))
        {
            return Encoding.GetEncoding(mediaType.Charset.Value);
        }

        return null;
    }

    private static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        return contentDisposition.DispositionType.Equals("form-data") &&
               string.IsNullOrWhiteSpace(contentDisposition.FileName.Value) &&
               string.IsNullOrWhiteSpace(contentDisposition.FileNameStar.Value);
    }

    private static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        return contentDisposition.DispositionType.Equals("form-data") &&
               (!string.IsNullOrWhiteSpace(contentDisposition.FileName.Value) ||
                !string.IsNullOrWhiteSpace(contentDisposition.FileNameStar.Value));
    }
}
