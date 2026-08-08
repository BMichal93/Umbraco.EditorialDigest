using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Services;

public static class EditorialDigestTemplatePathResolver
{
    private const string TemplateDirectory = "/Views/Partials/EditorialDigest/";
    private const string DefaultTemplatePath = TemplateDirectory + "EmailTemplate.cshtml";

    public static string Resolve(EditorialDigestConfig configuration)
    {
        if (IsAllowed(configuration.CustomTemplatePath))
        {
            return configuration.CustomTemplatePath!;
        }

        return TemplateDirectory + "EmailTemplate-" + configuration.Alias + ".cshtml";
    }

    public static bool IsAllowed(string? path)
        => !string.IsNullOrWhiteSpace(path)
           && path.StartsWith(TemplateDirectory, StringComparison.Ordinal)
           && path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
           && !path.Contains("..", StringComparison.Ordinal);

    public static string GetDefaultPath() => DefaultTemplatePath;
}
