using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.EditorialDigest.Composing;
using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Migrations;
using Umbraco.EditorialDigest.Settings;
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
    public void PackageMigrationCreatesTablesAndConfigStorePersistsConfiguration()
    {
        var migrationPlan = new EditorialDigestMigrationPlan();
        var migrationResult = GetRequiredService<IMigrationPlanExecutor>()
            .ExecutePlan(migrationPlan, migrationPlan.InitialState);

        Assert.That(migrationResult.Successful, Is.True, migrationResult.Exception?.ToString());

        using var scope = ScopeProvider.CreateScope();

        var tableCount = scope.Database.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN (@0, @1, @2, @3)",
            "umbracoEditorialDigestConfig",
            "umbracoEditorialDigestMailingList",
            "umbracoEditorialDigestLog",
            "umbracoEditorialDigestGlobalSettings");

        Assert.That(tableCount, Is.EqualTo(4));

        var store = GetRequiredService<IEditorialDigestConfigStore>();
        var id = store.Create(new DigestConfigRequest
        {
            Name = "Integration digest",
            Alias = "integration-digest",
            ScheduleTime = TimeSpan.FromHours(9),
            TimeZoneId = "UTC",
            SectionsEnabled = [DigestSection.RecentlyPublished]
        });

        var config = store.GetById(id);

        Assert.Multiple(() =>
        {
            Assert.That(config, Is.Not.Null);
            Assert.That(config!.Name, Is.EqualTo("Integration digest"));
            Assert.That(config.Alias, Is.EqualTo("integration-digest"));
        });
    }
}
