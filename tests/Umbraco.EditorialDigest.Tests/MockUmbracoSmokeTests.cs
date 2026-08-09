using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.EditorialDigest.Composing;
using Umbraco.EditorialDigest.Migrations;
using Umbraco.EditorialDigest.Services;

namespace Umbraco.EditorialDigest.Tests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public sealed class MockUmbracoSmokeTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => new EditorialDigestComposer().Compose(builder);

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var listener = new DiagnosticListener("EditorialDigestTests");
        services.AddSingleton(listener);
        services.AddSingleton<DiagnosticSource>(listener);
    }

    [Test]
    public void ScopeProviderIsAvailable()
    {
        Assert.That(ScopeProvider, Is.Not.Null);
        Assert.That(GetRequiredService<IEditorialDigestDataService>(), Is.Not.Null);
    }

    [Test]
    public void PackageMigrationPlanIsRegistered()
    {
        var plans = GetRequiredService<PackageMigrationPlanCollection>();

        Assert.That(plans.OfType<EditorialDigestMigrationPlan>(), Has.Exactly(1).Items);
    }
}
