using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.EditorialDigest.Domain;

namespace Umbraco.EditorialDigest.Persistence;

[TableName("umbracoEditorialDigestConfig")]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
public sealed class EditorialDigestConfig
{
    [Column("Id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("Name")]
    [Length(255)]
    public string Name { get; set; } = string.Empty;

    [Column("Alias")]
    [Length(255)]
    public string Alias { get; set; } = string.Empty;

    [Column("Description")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(2000)]
    public string? Description { get; set; }

    [Column("IsEnabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("RecipientSource")]
    public RecipientSource RecipientSource { get; set; }

    [Column("RecipientUserGroups")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(2000)]
    public string? RecipientUserGroups { get; set; }

    [Column("ScheduleType")]
    public ScheduleType ScheduleType { get; set; }

    [Column("ScheduleDay")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? ScheduleDay { get; set; }

    [Column("ScheduleTime")]
    public TimeSpan ScheduleTime { get; set; }

    [Column("TimeZoneId")]
    [Length(255)]
    public string TimeZoneId { get; set; } = "UTC";

    [Column("SectionsEnabled")]
    [Length(4000)]
    public string SectionsEnabled { get; set; } = "[]";

    [Column("LookbackHours")]
    public int LookbackHours { get; set; } = 24;

    [Column("UpcomingHours")]
    public int UpcomingHours { get; set; } = 48;

    [Column("StaleDays")]
    public int StaleDays { get; set; } = 90;

    [Column("ExpiringDays")]
    public int ExpiringDays { get; set; } = 7;

    [Column("MaxItemsPerSection")]
    public int MaxItemsPerSection { get; set; } = 10;

    [Column("SubjectLineTemplate")]
    [Length(500)]
    public string SubjectLineTemplate { get; set; } = "{{digestName}} — Editorial Digest for {{date}}";

    [Column("FromName")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(255)]
    public string? FromName { get; set; }

    [Column("FromEmail")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(320)]
    public string? FromEmail { get; set; }

    [Column("ReplyToEmail")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(320)]
    public string? ReplyToEmail { get; set; }

    [Column("CustomTemplatePath")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(1000)]
    public string? CustomTemplatePath { get; set; }

    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Column("LastModifiedDate")]
    public DateTime LastModifiedDate { get; set; }

    [Column("LastRunDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastRunDate { get; set; }

    [Column("LastRunStatus")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(32)]
    public string? LastRunStatus { get; set; }

    [Column("LastRunError")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(4000)]
    public string? LastRunError { get; set; }

    [Column("LastRunRecipientCount")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LastRunRecipientCount { get; set; }
}
