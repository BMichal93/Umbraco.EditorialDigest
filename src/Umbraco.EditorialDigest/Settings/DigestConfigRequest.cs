using System.ComponentModel.DataAnnotations;
using Umbraco.EditorialDigest.Domain;

namespace Umbraco.EditorialDigest.Settings;

public sealed class DigestConfigRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Alias { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    public bool IsEnabled { get; init; } = true;
    public RecipientSource RecipientSource { get; init; }

    [StringLength(2000)]
    public string? RecipientUserGroups { get; init; }

    public ScheduleType ScheduleType { get; init; }
    public int? ScheduleDay { get; init; }
    public TimeSpan ScheduleTime { get; init; }

    [Required]
    [StringLength(255)]
    public string TimeZoneId { get; init; } = "UTC";

    public IReadOnlyCollection<DigestSection> SectionsEnabled { get; init; } = [];

    [Range(1, 720)]
    public int LookbackHours { get; init; } = 24;

    [Range(1, 720)]
    public int UpcomingHours { get; init; } = 48;

    [Range(1, 3650)]
    public int StaleDays { get; init; } = 90;

    [Range(1, 365)]
    public int ExpiringDays { get; init; } = 7;

    [Range(1, 50)]
    public int MaxItemsPerSection { get; init; } = 10;

    [Required]
    [StringLength(500)]
    public string SubjectLineTemplate { get; init; } = "{{digestName}} — Editorial Digest for {{date}}";

    [StringLength(255)]
    public string? FromName { get; init; }

    [EmailAddress]
    [StringLength(320)]
    public string? FromEmail { get; init; }

    [EmailAddress]
    [StringLength(320)]
    public string? ReplyToEmail { get; init; }

    [StringLength(1000)]
    public string? CustomTemplatePath { get; init; }
}
