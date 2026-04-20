using System.Text.RegularExpressions;

namespace CloudDocs.Application.Common.Helpers;

/// <summary>
/// Builds logical storage paths for document files.
/// </summary>
public static class StoragePathBuilder
{
    /// <summary>
    /// Builds a storage path for a document based on client, category, and document type.
    /// </summary>
    public static string BuildClientCategoryPath(
        string clientName,
        string categoryName,
        string documentTypeName,
        string fileName,
        DateTime? uploadDateUtc = null)
    {
        var baseDirectory = BuildClientCategoryDocumentTypeDirectory(
            clientName,
            categoryName,
            documentTypeName,
            uploadDateUtc);

        return Path.Combine(baseDirectory, fileName);
    }

    /// <summary>
    /// Builds a storage path for a document version under the same client/category/document type hierarchy.
    /// </summary>
    public static string BuildVersionPath(
        string clientName,
        string categoryName,
        string documentTypeName,
        Guid documentId,
        string fileName,
        DateTime? uploadDateUtc = null)
    {
        var baseDirectory = BuildClientCategoryDocumentTypeDirectory(
            clientName,
            categoryName,
            documentTypeName,
            uploadDateUtc);

        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            var effectiveDate = uploadDateUtc ?? DateTime.UtcNow;

            return Path.Combine(
                effectiveDate.Year.ToString("0000"),
                effectiveDate.Month.ToString("00"),
                "documents",
                documentId.ToString("N"),
                "versions",
                fileName);
        }

        return Path.Combine(baseDirectory, "versions", documentId.ToString("N"), fileName);
    }

    /// <summary>
    /// Builds a storage path for a document version under the same client/category hierarchy.
    /// </summary>
    /// <param name="currentDocumentPath">The current document storage path.</param>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The relative version storage path.</returns>
    public static string BuildVersionPath(
        string currentDocumentPath,
        Guid documentId,
        string fileName,
        DateTime? uploadDateUtc = null)
    {
        var effectiveDate = uploadDateUtc ?? DateTime.UtcNow;
        var currentDirectory = Path.GetDirectoryName(currentDocumentPath);

        if (string.IsNullOrWhiteSpace(currentDirectory))
        {
            return Path.Combine(
                effectiveDate.Year.ToString("0000"),
                effectiveDate.Month.ToString("00"),
                "documents",
                documentId.ToString("N"),
                "versions",
                fileName);
        }

        var baseDirectory = HasYearMonthPrefix(currentDirectory)
            ? currentDirectory
            : Path.Combine(
                effectiveDate.Year.ToString("0000"),
                effectiveDate.Month.ToString("00"),
                currentDirectory);

        return Path.Combine(baseDirectory, "versions", documentId.ToString("N"), fileName);
    }

    /// <summary>
    /// Converts a string into a safe slug for file paths.
    /// </summary>
    private static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unknown";

        var normalized = value.Trim().ToLowerInvariant();

        // espacios → guiones
        normalized = Regex.Replace(normalized, @"\s+", "-");

        // eliminar caracteres raros
        normalized = Regex.Replace(normalized, @"[^a-z0-9\-]", "");

        // evitar dobles guiones
        normalized = Regex.Replace(normalized, @"\-{2,}", "-");

        return normalized.Trim('-');
    }

    private static string FormatDocumentTypeSegment(string value)
    {
        return Slugify(value).ToUpperInvariant();
    }

    private static string BuildClientCategoryDocumentTypeDirectory(
        string clientName,
        string categoryName,
        string documentTypeName,
        DateTime? uploadDateUtc = null)
    {
        var clientSlug = Slugify(clientName);
        var categorySlug = Slugify(categoryName);
        var documentTypeSegment = FormatDocumentTypeSegment(documentTypeName);
        var effectiveDate = uploadDateUtc ?? DateTime.UtcNow;

        return Path.Combine(
            effectiveDate.Year.ToString("0000"),
            effectiveDate.Month.ToString("00"),
            "Client",
            clientSlug,
            categorySlug,
            documentTypeSegment);
    }

    private static bool HasYearMonthPrefix(string directoryPath)
    {
        var segments = directoryPath
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .ToArray();

        if (segments.Length < 2)
            return false;

        var yearSegment = segments[0];
        var monthSegment = segments[1];

        return yearSegment.Length == 4 &&
               int.TryParse(yearSegment, out var year) &&
               year >= 2000 &&
               monthSegment.Length == 2 &&
               int.TryParse(monthSegment, out var month) &&
               month is >= 1 and <= 12;
    }
}
