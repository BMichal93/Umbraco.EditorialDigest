namespace Umbraco.EditorialDigest.Settings;

public sealed class DigestConfigSummary
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Alias { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public DateTime? LastRunDate { get; init; }
    public string? LastRunStatus { get; init; }
    public int? LastRunRecipientCount { get; init; }
}
