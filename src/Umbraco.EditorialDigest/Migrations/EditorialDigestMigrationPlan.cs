using Umbraco.Cms.Core.Packaging;

namespace Umbraco.EditorialDigest.Migrations;

public sealed class EditorialDigestMigrationPlan : PackageMigrationPlan
{
    public EditorialDigestMigrationPlan()
        : base("Editorial Digest")
    {
    }

    protected override void DefinePlan()
        => To<CreateEditorialDigestTablesMigration>(new Guid("2dd70eb6-116a-451d-9461-7d27a2f24290"));
}
