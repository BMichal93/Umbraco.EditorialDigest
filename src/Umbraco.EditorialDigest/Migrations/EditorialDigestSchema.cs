using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.EditorialDigest.Migrations;

[TableName("umbracoEditorialDigestConfig")]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
internal sealed class EditorialDigestConfigSchema
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
    public bool IsEnabled { get; set; }

    [Column("RecipientSource")]
    public int RecipientSource { get; set; }

    [Column("RecipientUserGroups")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(2000)]
    public string? RecipientUserGroups { get; set; }

    [Column("ScheduleType")]
    public int ScheduleType { get; set; }

    [Column("ScheduleDay")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? ScheduleDay { get; set; }

    [Column("ScheduleTime")]
    public long ScheduleTimeTicks { get; set; }

    [Column("TimeZoneId")]
    [Length(255)]
    public string TimeZoneId { get; set; } = string.Empty;

    [Column("SectionsEnabled")]
    [Length(4000)]
    public string SectionsEnabled { get; set; } = string.Empty;

    [Column("LookbackHours")]
    public int LookbackHours { get; set; }

    [Column("UpcomingHours")]
    public int UpcomingHours { get; set; }

    [Column("StaleDays")]
    public int StaleDays { get; set; }

    [Column("ExpiringDays")]
    public int ExpiringDays { get; set; }

    [Column("MaxItemsPerSection")]
    public int MaxItemsPerSection { get; set; }

    [Column("SubjectLineTemplate")]
    [Length(500)]
    public string SubjectLineTemplate { get; set; } = string.Empty;

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

[TableName("umbracoEditorialDigestGlobalSettings")]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
internal sealed class EditorialDigestGlobalSettingsSchema
{
    [Column("Id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("DefaultFromName")]
    [Length(255)]
    public string DefaultFromName { get; set; } = string.Empty;

    [Column("DefaultFromEmail")]
    [Length(320)]
    public string DefaultFromEmail { get; set; } = string.Empty;

    [Column("LogoUrl")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(2000)]
    public string? LogoUrl { get; set; }

    [Column("CustomTemplateBasePath")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(1000)]
    public string? CustomTemplateBasePath { get; set; }

    [Column("DashboardRefreshMinutes")]
    public int DashboardRefreshMinutes { get; set; }

    [Column("IsPackageEnabled")]
    public bool IsPackageEnabled { get; set; }

    [Column("LoggingLevel")]
    public int LoggingLevel { get; set; }
}
