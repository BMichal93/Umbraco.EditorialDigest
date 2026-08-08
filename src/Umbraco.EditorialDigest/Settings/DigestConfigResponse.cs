using Umbraco.EditorialDigest.Domain;

namespace Umbraco.EditorialDigest.Settings;

public sealed class DigestConfigResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Alias { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsEnabled { get; init; }
    public RecipientSource RecipientSource { get; init; }
    public string? RecipientUserGroups { get; init; }
    public ScheduleType ScheduleType { get; init; }
    public int? ScheduleDay { get; init; }
    public TimeSpan ScheduleTime { get; init; }
    public string TimeZoneId { get; init; } = "UTC";
    public IReadOnlyCollection<DigestSection> SectionsEnabled { get; init; } = [];
    public int LookbackHours { get; init; }
    public int UpcomingHours { get; init; }
    public int StaleDays { get; init; }
    public int ExpiringDays { get; init; }
    public int MaxItemsPerSection { get; init; }
    public string SubjectLineTemplate { get; init; } = string.Empty;
    public string? FromName { get; init; }
    public string? FromEmail { get; init; }
    public string? ReplyToEmail { get; init; }
    public string? CustomTemplatePath { get; init; }
    public DateTime? LastRunDate { get; init; }
    public string? LastRunStatus { get; init; }
    public int? LastRunRecipientCount { get; init; }
}
