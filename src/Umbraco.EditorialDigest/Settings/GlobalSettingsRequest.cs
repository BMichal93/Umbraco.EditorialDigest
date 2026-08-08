using System.ComponentModel.DataAnnotations;
using Umbraco.EditorialDigest.Domain;

namespace Umbraco.EditorialDigest.Settings;

public sealed class GlobalSettingsRequest
{
    [StringLength(255)]
    public string DefaultFromName { get; init; } = string.Empty;

    [EmailAddress]
    [StringLength(320)]
    public string? DefaultFromEmail { get; init; }

    [Url]
    [StringLength(2000)]
    public string? LogoUrl { get; init; }

    [StringLength(1000)]
    public string? CustomTemplateBasePath { get; init; }

    [Range(1, 60)]
    public int DashboardRefreshMinutes { get; init; } = 5;

    public bool IsPackageEnabled { get; init; } = true;

    [EnumDataType(typeof(DigestLoggingLevel))]
    public DigestLoggingLevel LoggingLevel { get; init; } = DigestLoggingLevel.Normal;
}
