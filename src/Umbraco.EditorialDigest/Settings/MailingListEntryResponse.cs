namespace Umbraco.EditorialDigest.Settings;

public sealed class MailingListEntryResponse
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Company { get; init; }
    public bool IsActive { get; init; }
}
