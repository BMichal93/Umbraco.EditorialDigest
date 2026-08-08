using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.EditorialDigest.Domain;

namespace Umbraco.EditorialDigest.Persistence;

[TableName("umbracoEditorialDigestGlobalSettings")]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
public sealed class EditorialDigestGlobalSettings
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
    public int DashboardRefreshMinutes { get; set; } = 5;

    [Column("IsPackageEnabled")]
    public bool IsPackageEnabled { get; set; } = true;

    [Column("LoggingLevel")]
    public DigestLoggingLevel LoggingLevel { get; set; } = DigestLoggingLevel.Normal;
}
