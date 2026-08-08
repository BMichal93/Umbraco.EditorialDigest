using System.ComponentModel.DataAnnotations;

namespace Umbraco.EditorialDigest.Settings;

public static class MailingListValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(MailingListEntryRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty), (result, memberName) => new { memberName, result.ErrorMessage })
            .GroupBy(item => item.memberName)
            .ToDictionary(group => group.Key, group => group.Select(item => item.ErrorMessage ?? "Invalid value.").ToArray());
    }
}
