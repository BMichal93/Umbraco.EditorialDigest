using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.EditorialDigest.MockUmbraco;

public sealed class EditorialDigestMockComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.PackageMigrationPlans().Add<EditorialDigestMockSeedPlan>();
}
