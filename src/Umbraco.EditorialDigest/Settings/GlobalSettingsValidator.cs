using System.ComponentModel.DataAnnotations;

namespace Umbraco.EditorialDigest.Settings;

public static class GlobalSettingsValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(GlobalSettingsRequest request)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);
        Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);

        if (!string.IsNullOrWhiteSpace(request.CustomTemplateBasePath) && !request.CustomTemplateBasePath.StartsWith("/Views/", StringComparison.Ordinal))
        {
            validationResults.Add(new ValidationResult("The template path must be rooted under /Views/.", [nameof(request.CustomTemplateBasePath)]));
        }

        return validationResults
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty), (result, memberName) => new { memberName, result.ErrorMessage })
            .GroupBy(item => item.memberName)
            .ToDictionary(group => group.Key, group => group.Select(item => item.ErrorMessage ?? "Invalid value.").ToArray());
    }
}
