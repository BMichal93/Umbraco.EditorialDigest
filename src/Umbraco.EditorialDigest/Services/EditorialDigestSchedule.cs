using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Services;

public static class EditorialDigestSchedule
{
    public static bool IsDue(EditorialDigestConfig configuration, DateTime utcNow)
    {
        if (!configuration.IsEnabled || configuration.LastRunDate is { } lastRun && lastRun >= utcNow.Date)
        {
            return false;
        }

        var timeZone = GetTimeZone(configuration.TimeZoneId);
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        if (configuration.ScheduleType == ScheduleType.Weekly && configuration.ScheduleDay != (int)localNow.DayOfWeek)
        {
            return false;
        }

        if (localNow.TimeOfDay < configuration.ScheduleTime)
        {
            return false;
        }

        return configuration.LastRunDate is null || TimeZoneInfo.ConvertTimeFromUtc(configuration.LastRunDate.Value, timeZone).Date < localNow.Date;
    }

    private static TimeZoneInfo GetTimeZone(string timeZoneId)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
        catch (InvalidTimeZoneException) { return TimeZoneInfo.Utc; }
    }
}
