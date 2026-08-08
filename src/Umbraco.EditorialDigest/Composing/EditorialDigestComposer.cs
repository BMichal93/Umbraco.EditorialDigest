using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.EditorialDigest.Settings;
using Umbraco.EditorialDigest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.EditorialDigest.Composing;

public sealed class EditorialDigestComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IGlobalSettingsStore, GlobalSettingsStore>();
        builder.Services.AddSingleton<IEditorialDigestConfigStore, EditorialDigestConfigStore>();
        builder.Services.AddSingleton<IMailingListStore, MailingListStore>();
        builder.Services.AddSingleton<IEditorialDigestDataService, EditorialDigestDataService>();
        builder.Services.AddSingleton<IEditorialDigestEmailRenderer, RazorEditorialDigestEmailRenderer>();
    }
}
