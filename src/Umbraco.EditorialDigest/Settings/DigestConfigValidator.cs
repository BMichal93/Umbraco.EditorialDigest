using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Umbraco.EditorialDigest.Domain;

namespace Umbraco.EditorialDigest.Settings;

public static partial class DigestConfigValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(DigestConfigRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);

        if (!AliasPattern().IsMatch(request.Alias.Trim()))
        {
            results.Add(new ValidationResult("Alias must start with a letter and contain only lowercase letters, numbers, and hyphens.", [nameof(request.Alias)]));
        }

        if (request.ScheduleType == ScheduleType.Weekly && request.ScheduleDay is not >= 0 or > 6)
        {
            results.Add(new ValidationResult("A weekday is required for weekly digests.", [nameof(request.ScheduleDay)]));
        }

        if (request.ScheduleType == ScheduleType.Daily && request.ScheduleDay is not null)
        {
            results.Add(new ValidationResult("Daily digests cannot specify a weekday.", [nameof(request.ScheduleDay)]));
        }

        if (request.ScheduleTime < TimeSpan.Zero || request.ScheduleTime >= TimeSpan.FromDays(1))
        {
            results.Add(new ValidationResult("Schedule time must be within a single day.", [nameof(request.ScheduleTime)]));
        }

        if (!IsKnownTimeZone(request.TimeZoneId))
        {
            results.Add(new ValidationResult("Time zone is not available on this server.", [nameof(request.TimeZoneId)]));
        }

        if (request.SectionsEnabled.Count == 0)
        {
            results.Add(new ValidationResult("Select at least one digest section.", [nameof(request.SectionsEnabled)]));
        }

        if (request.SectionsEnabled.Distinct().Count() != request.SectionsEnabled.Count)
        {
            results.Add(new ValidationResult("Digest sections cannot be repeated.", [nameof(request.SectionsEnabled)]));
        }

        if (!string.IsNullOrWhiteSpace(request.CustomTemplatePath) && !request.CustomTemplatePath.StartsWith("/Views/", StringComparison.Ordinal))
        {
            results.Add(new ValidationResult("The template path must be rooted under /Views/.", [nameof(request.CustomTemplatePath)]));
        }

        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty), (result, memberName) => new { memberName, result.ErrorMessage })
            .GroupBy(item => item.memberName)
            .ToDictionary(group => group.Key, group => group.Select(item => item.ErrorMessage ?? "Invalid value.").ToArray());
    }

    private static bool IsKnownTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return false;
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }

    [GeneratedRegex("^[a-z][a-z0-9-]*$")]
    private static partial Regex AliasPattern();
}
