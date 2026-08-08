using System.ComponentModel.DataAnnotations;

namespace Umbraco.EditorialDigest.Settings;

public sealed class MailingListEntryRequest
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; init; } = string.Empty;

    [StringLength(255)]
    public string? Name { get; init; }

    [StringLength(255)]
    public string? Company { get; init; }

    public bool IsActive { get; init; } = true;
}
