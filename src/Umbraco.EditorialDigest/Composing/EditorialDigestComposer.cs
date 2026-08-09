using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.EditorialDigest.Migrations;
using Umbraco.EditorialDigest.Settings;
using Umbraco.EditorialDigest.Services;
using Umbraco.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.EditorialDigest.Composing;

public sealed class EditorialDigestComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.PackageMigrationPlans().Add<EditorialDigestMigrationPlan>();
        builder.Services.AddSingleton<IGlobalSettingsStore, GlobalSettingsStore>();
        builder.Services.AddSingleton<IEditorialDigestConfigStore, EditorialDigestConfigStore>();
        builder.Services.AddSingleton<IMailingListStore, MailingListStore>();
        builder.Services.AddSingleton<IEditorialDigestLogStore, EditorialDigestLogStore>();
        builder.Services.AddSingleton<IEditorialDigestDataService, EditorialDigestDataService>();
        builder.Services.AddSingleton<IEditorialDigestEmailRenderer, RazorEditorialDigestEmailRenderer>();
        builder.Services.AddSingleton<IEditorialDigestDeliveryService, EditorialDigestDeliveryService>();
        builder.Services.AddHostedService<EditorialDigestScheduler>();
    }
}
