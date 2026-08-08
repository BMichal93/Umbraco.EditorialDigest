using Microsoft.AspNetCore.Authorization;
using NUnit.Framework;
using Umbraco.EditorialDigest.Controllers;

namespace Umbraco.EditorialDigest.Tests;

[TestFixture]
public sealed class GlobalSettingsAuthorizationTests
{
    [Test]
    public void ControllerUsesUmbracosBuiltInAdministratorGroupAlias()
    {
        var attribute = typeof(GlobalSettingsApiController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.That(attribute.Roles, Is.EqualTo(Umbraco.Cms.Core.Constants.Security.AdminGroupAlias));
    }
}
