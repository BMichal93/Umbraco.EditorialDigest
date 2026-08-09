using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.EditorialDigest.MockUmbraco.Migrations;

internal sealed class CreateMockContentMigration(
    IMigrationContext context,
    IContentService contentService,
    IContentTypeService contentTypeService,
    IContentPublishingService contentPublishingService) : AsyncMigrationBase(context)
{
    private static readonly CulturePublishScheduleModel[] PublishInvariant = [new() { Culture = null }];

    protected override async Task MigrateAsync()
    {
        IContentType contentType = contentTypeService.Get(EditorialDigestMockContent.ContentTypeAlias)
            ?? throw new InvalidOperationException("The editorial mock document type was not created.");

        var ids = new Dictionary<string, int>();
        foreach (MockContentItem item in EditorialDigestMockContent.Items)
        {
            int parentId = item.ParentName is null ? -1 : ids[item.ParentName];
            IContent content = contentService.Create(item.Name, parentId, contentType);
            content.SetValue("summary", item.Summary);
            contentService.Save(content);

            if (item.IsPublished)
            {
                await contentPublishingService.PublishAsync(content.Key, PublishInvariant, global::Umbraco.Cms.Core.Constants.Security.SuperUserKey);
            }

            ids[item.Name] = content.Id;
        }
    }
}
