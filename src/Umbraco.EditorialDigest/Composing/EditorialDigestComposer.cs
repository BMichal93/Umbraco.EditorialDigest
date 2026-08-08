using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.EditorialDigest.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.EditorialDigest.Composing;

public sealed class EditorialDigestComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.Services.AddSingleton<IGlobalSettingsStore, GlobalSettingsStore>();
}
