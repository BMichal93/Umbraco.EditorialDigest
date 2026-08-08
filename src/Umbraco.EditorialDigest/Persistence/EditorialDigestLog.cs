using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.EditorialDigest.Persistence;

[TableName("umbracoEditorialDigestLog")]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
public sealed class EditorialDigestLog
{
    [Column("Id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("ConfigId")]
    public int ConfigId { get; set; }

    [Column("SentDate")]
    public DateTime SentDate { get; set; }

    [Column("RecipientCount")]
    public int RecipientCount { get; set; }

    [Column("Status")]
    [Length(32)]
    public string Status { get; set; } = string.Empty;

    [Column("ErrorMessage")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(4000)]
    public string? ErrorMessage { get; set; }

    [Column("DurationMs")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? DurationMs { get; set; }
}
