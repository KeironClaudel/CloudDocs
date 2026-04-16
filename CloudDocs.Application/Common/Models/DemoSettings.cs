namespace CloudDocs.Application.Common.Models;

/// <summary>
/// Represents public demo restrictions for the application.
/// </summary>
public class DemoSettings
{
    public const string SectionName = "Demo";

    public bool Enabled { get; set; }
    public string DemoUserEmail { get; set; } = string.Empty;
    public int MaxDocumentsPerDemoUser { get; set; } = 2;
    public int MaxEmailsPerDemoUser { get; set; } = 2;
    public long MaxDemoFileSizeInBytes { get; set; } = 1048576; // 1 MB
    public bool PdfOnly { get; set; } = true;
}