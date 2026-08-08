using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.EditorialDigest.Persistence;

[TableName("umbracoEditorialDigestMailingList")]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
public sealed class EditorialDigestMailingListEntry
{
    [Column("Id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("ConfigId")]
    public int ConfigId { get; set; }

    [Column("Email")]
    [Length(320)]
    public string Email { get; set; } = string.Empty;

    [Column("Name")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(255)]
    public string? Name { get; set; }

    [Column("Company")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(255)]
    public string? Company { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("UnsubscribeToken")]
    [Length(64)]
    public string UnsubscribeToken { get; set; } = string.Empty;

    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Column("LastModifiedDate")]
    public DateTime LastModifiedDate { get; set; }
}
