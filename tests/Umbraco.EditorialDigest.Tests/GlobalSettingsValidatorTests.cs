using NUnit.Framework;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Tests;

[TestFixture]
public sealed class GlobalSettingsValidatorTests
{
    [Test]
    public void ValidateAllowsAnEmptyDefaultSenderForInitialSetup()
    {
        var errors = GlobalSettingsValidator.Validate(new GlobalSettingsRequest());

        Assert.That(errors, Is.Empty);
    }

    [TestCase("../Views/Partials/EditorialDigest")]
    [TestCase("C:\\Views\\Partials")]
    [TestCase("/App_Data/template.cshtml")]
    public void ValidateRejectsTemplatePathsOutsideTheViewsDirectory(string path)
    {
        var errors = GlobalSettingsValidator.Validate(new GlobalSettingsRequest { CustomTemplateBasePath = path });

        Assert.That(errors, Does.ContainKey(nameof(GlobalSettingsRequest.CustomTemplateBasePath)));
    }

    [Test]
    public void ValidateAllowsATemplatePathUnderViews()
    {
        var errors = GlobalSettingsValidator.Validate(new GlobalSettingsRequest
        {
            CustomTemplateBasePath = "/Views/Partials/EditorialDigest"
        });

        Assert.That(errors, Is.Empty);
    }
}
