using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.EditorialDigest.MockUmbraco.Migrations;

internal sealed class CreateMockDocumentTypeMigration(
    IMigrationContext context,
    IContentTypeService contentTypeService,
    IDataTypeService dataTypeService,
    IShortStringHelper shortStringHelper) : AsyncMigrationBase(context)
{
    protected override async Task MigrateAsync()
    {
        IDataType textBox = (await dataTypeService.GetByEditorAliasAsync("Umbraco.TextBox")).First();
        var contentType = new ContentType(shortStringHelper, -1)
        {
            Alias = EditorialDigestMockContent.ContentTypeAlias,
            Name = "Editorial Page",
            Icon = "icon-document",
            AllowedAsRoot = true
        };

        var propertyType = new PropertyType(shortStringHelper, textBox)
        {
            Alias = "summary",
            Name = "Summary"
        };

        contentType.PropertyGroups.Add(new PropertyGroup(new PropertyTypeCollection(false) { propertyType })
        {
            Name = "Content",
            Alias = "content",
            Type = PropertyGroupType.Group
        });

        await contentTypeService.CreateAsync(contentType, global::Umbraco.Cms.Core.Constants.Security.SuperUserKey);

        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await contentTypeService.UpdateAsync(contentType, global::Umbraco.Cms.Core.Constants.Security.SuperUserKey);
    }
}
