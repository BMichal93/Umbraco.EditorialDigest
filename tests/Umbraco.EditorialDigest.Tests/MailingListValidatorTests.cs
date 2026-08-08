using NUnit.Framework;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Tests;

[TestFixture]
public sealed class MailingListValidatorTests
{
    [Test]
    public void ValidateAcceptsAnActiveRecipient()
    {
        var errors = MailingListValidator.Validate(new MailingListEntryRequest
        {
            Email = "editor@example.com",
            Name = "Editor",
            IsActive = true
        });

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void ValidateRejectsAnInvalidEmailAddress()
    {
        var errors = MailingListValidator.Validate(new MailingListEntryRequest { Email = "not-an-email" });

        Assert.That(errors, Does.ContainKey(nameof(MailingListEntryRequest.Email)));
    }
}
