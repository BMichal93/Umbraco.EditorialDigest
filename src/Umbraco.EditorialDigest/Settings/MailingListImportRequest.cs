using System.ComponentModel.DataAnnotations;

namespace Umbraco.EditorialDigest.Settings;

public sealed class MailingListImportRequest
{
    [Required]
    [StringLength(100000)]
    public string Values { get; init; } = string.Empty;
}
