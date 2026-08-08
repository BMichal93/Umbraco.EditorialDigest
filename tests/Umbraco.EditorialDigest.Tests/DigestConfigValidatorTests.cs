using NUnit.Framework;
using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Tests;

[TestFixture]
public sealed class DigestConfigValidatorTests
{
    [Test]
    public void ValidateAcceptsACompleteDailyConfiguration()
    {
        var errors = DigestConfigValidator.Validate(CreateValidRequest());

        Assert.That(errors, Is.Empty);
    }

    [TestCase("Marketing Daily")]
    [TestCase("marketing_daily")]
    [TestCase("9marketing")]
    public void ValidateRejectsUnsafeAliases(string alias)
    {
        var errors = DigestConfigValidator.Validate(CreateValidRequest(alias: alias));

        Assert.That(errors, Does.ContainKey(nameof(DigestConfigRequest.Alias)));
    }

    [Test]
    public void ValidateRequiresAWeekdayForWeeklyDigests()
    {
        var errors = DigestConfigValidator.Validate(CreateValidRequest(scheduleType: ScheduleType.Weekly));

        Assert.That(errors, Does.ContainKey(nameof(DigestConfigRequest.ScheduleDay)));
    }

    [Test]
    public void ValidateRejectsTemplatePathsOutsideViews()
    {
        var errors = DigestConfigValidator.Validate(CreateValidRequest(customTemplatePath: "../Views/EmailTemplate.cshtml"));

        Assert.That(errors, Does.ContainKey(nameof(DigestConfigRequest.CustomTemplatePath)));
    }

    private static DigestConfigRequest CreateValidRequest(
        string alias = "marketing-daily",
        ScheduleType scheduleType = ScheduleType.Daily,
        string? customTemplatePath = null)
        => new()
        {
            Name = "Marketing Daily",
            Alias = alias,
            ScheduleType = scheduleType,
            ScheduleTime = TimeSpan.FromHours(9),
            TimeZoneId = "UTC",
            SectionsEnabled = [DigestSection.RecentlyPublished],
            CustomTemplatePath = customTemplatePath
        };
}
