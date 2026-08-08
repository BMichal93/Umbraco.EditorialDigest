using NUnit.Framework;
using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Persistence;
using Umbraco.EditorialDigest.Services;

namespace Umbraco.EditorialDigest.Tests;

[TestFixture]
public sealed class EditorialDigestScheduleTests
{
    [Test]
    public void IsDueWhenDailyScheduleTimeHasPassedAndItHasNotRunToday()
    {
        var configuration = new EditorialDigestConfig
        {
            IsEnabled = true,
            ScheduleType = ScheduleType.Daily,
            ScheduleTime = TimeSpan.FromHours(9),
            TimeZoneId = "UTC",
            LastRunDate = new DateTime(2026, 8, 7, 9, 0, 0, DateTimeKind.Utc)
        };

        Assert.That(EditorialDigestSchedule.IsDue(configuration, new DateTime(2026, 8, 8, 9, 1, 0, DateTimeKind.Utc)), Is.True);
    }

    [Test]
    public void IsNotDueWhenItHasAlreadyRunOnTheLocalDate()
    {
        var configuration = new EditorialDigestConfig
        {
            IsEnabled = true,
            ScheduleType = ScheduleType.Daily,
            ScheduleTime = TimeSpan.FromHours(9),
            TimeZoneId = "UTC",
            LastRunDate = new DateTime(2026, 8, 8, 9, 0, 0, DateTimeKind.Utc)
        };

        Assert.That(EditorialDigestSchedule.IsDue(configuration, new DateTime(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc)), Is.False);
    }

    [Test]
    public void IsNotDueOnAnotherWeeklyDay()
    {
        var configuration = new EditorialDigestConfig
        {
            IsEnabled = true,
            ScheduleType = ScheduleType.Weekly,
            ScheduleDay = (int)DayOfWeek.Monday,
            ScheduleTime = TimeSpan.Zero,
            TimeZoneId = "UTC"
        };

        Assert.That(EditorialDigestSchedule.IsDue(configuration, new DateTime(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc)), Is.False);
    }
}
