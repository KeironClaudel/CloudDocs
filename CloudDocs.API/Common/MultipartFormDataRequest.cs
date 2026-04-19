namespace CloudDocs.API.Common;

/// <summary>
/// Represents multipart form-data parsed from a streamed HTTP request.
/// </summary>
public sealed class MultipartFormDataRequest : IAsyncDisposable
{
    private readonly Dictionary<string, List<string>> _fields = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the uploaded file metadata.
    /// </summary>
    public StreamedFormFile? File { get; set; }

    /// <summary>
    /// Adds a form field value.
    /// </summary>
    /// <param name="key">The field name.</param>
    /// <param name="value">The field value.</param>
    public void AddField(string key, string value)
    {
        if (!_fields.TryGetValue(key, out var values))
        {
            values = new List<string>();
            _fields[key] = values;
        }

        values.Add(value);
    }

    /// <summary>
    /// Gets the first value for the specified field.
    /// </summary>
    /// <param name="key">The field name.</param>
    /// <returns>The field value when present; otherwise, null.</returns>
    public string? GetValue(string key)
    {
        return _fields.TryGetValue(key, out var values) ? values.FirstOrDefault() : null;
    }

    /// <summary>
    /// Gets all values for the specified logical field name, including indexed variants.
    /// </summary>
    /// <param name="key">The field name.</param>
    /// <returns>The field values.</returns>
    public IReadOnlyList<string> GetValues(string key)
    {
        var results = new List<string>();

        if (_fields.TryGetValue(key, out var directValues))
        {
            results.AddRange(directValues);
        }

        var indexedPrefix = $"{key}[";

        foreach (var pair in _fields)
        {
            if (pair.Key.StartsWith(indexedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                results.AddRange(pair.Value);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (File is not null)
        {
            await File.DisposeAsync();
        }
    }
}
