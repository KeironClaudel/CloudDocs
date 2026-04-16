using System.Text.RegularExpressions;

namespace CloudDocs.Application.Common.Helpers;

/// <summary>
/// Builds logical storage paths for document files.
/// </summary>
public static class StoragePathBuilder
{
    /// <summary>
    /// Builds a storage path for a document based on client and category.
    /// </summary>
    public static string BuildClientCategoryPath(
        string clientName,
        string categoryName,
        string fileName)
    {
        var clientSlug = Slugify(clientName);
        var categorySlug = Slugify(categoryName);

        return Path.Combine("clients", clientSlug, categorySlug, fileName);
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
}