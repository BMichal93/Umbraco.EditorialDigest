using Umbraco.Cms.Core.Packaging;
using Umbraco.EditorialDigest.MockUmbraco.Migrations;

namespace Umbraco.EditorialDigest.MockUmbraco;

public sealed class EditorialDigestMockSeedPlan : PackageMigrationPlan
{
    public EditorialDigestMockSeedPlan()
        : base("Umbraco.EditorialDigest.MockSeed")
    {
    }

    protected override void DefinePlan()
    {
        From(InitialState)
            .To<CreateMockDocumentTypeMigration>("editorial-digest-mock-doctype")
            .To<CreateMockContentMigration>("editorial-digest-mock-content");
    }
}
