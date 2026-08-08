using NUnit.Framework;
using Umbraco.EditorialDigest.Persistence;
using Umbraco.EditorialDigest.Services;

namespace Umbraco.EditorialDigest.Tests;

[TestFixture]
public sealed class EditorialDigestTemplatePathResolverTests
{
    [Test]
    public void ResolveUsesAliasSpecificTemplateWhenNoCustomPathIsConfigured()
    {
        var path = EditorialDigestTemplatePathResolver.Resolve(new EditorialDigestConfig { Alias = "marketing-daily" });

        Assert.That(path, Is.EqualTo("/Views/Partials/EditorialDigest/EmailTemplate-marketing-daily.cshtml"));
    }

    [TestCase("../Views/EmailTemplate.cshtml")]
    [TestCase("/App_Plugins/EditorialDigest/EmailTemplate.cshtml")]
    [TestCase("/Views/Partials/EditorialDigest/../EmailTemplate.cshtml")]
    public void ResolveRejectsPathsOutsideTheTemplateDirectory(string path)
    {
        var resolved = EditorialDigestTemplatePathResolver.Resolve(new EditorialDigestConfig { Alias = "daily", CustomTemplatePath = path });

        Assert.That(resolved, Is.EqualTo("/Views/Partials/EditorialDigest/EmailTemplate-daily.cshtml"));
    }
}
