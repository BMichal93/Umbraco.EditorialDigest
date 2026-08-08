using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Settings;

public sealed class GlobalSettingsStore : IGlobalSettingsStore
{
    private readonly IScopeProvider _scopeProvider;

    public GlobalSettingsStore(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public EditorialDigestGlobalSettings GetCurrent()
    {
        using var scope = _scopeProvider.CreateScope();
        return scope.Database.FirstOrDefault<EditorialDigestGlobalSettings>("ORDER BY Id") ?? new EditorialDigestGlobalSettings();
    }

    public void Save(GlobalSettingsRequest request)
    {
        using var scope = _scopeProvider.CreateScope();
        var settings = scope.Database.FirstOrDefault<EditorialDigestGlobalSettings>("ORDER BY Id");

        if (settings is null)
        {
            settings = new EditorialDigestGlobalSettings();
            Apply(request, settings);
            scope.Database.Insert(settings);
        }
        else
        {
            Apply(request, settings);
            scope.Database.Update(settings);
        }

        scope.Complete();
    }

    private static void Apply(GlobalSettingsRequest request, EditorialDigestGlobalSettings settings)
    {
        settings.DefaultFromName = request.DefaultFromName.Trim();
        settings.DefaultFromEmail = request.DefaultFromEmail?.Trim() ?? string.Empty;
        settings.LogoUrl = NormalizeOptionalValue(request.LogoUrl);
        settings.CustomTemplateBasePath = NormalizeOptionalValue(request.CustomTemplateBasePath);
        settings.DashboardRefreshMinutes = request.DashboardRefreshMinutes;
        settings.IsPackageEnabled = request.IsPackageEnabled;
        settings.LoggingLevel = request.LoggingLevel;
    }

    private static string? NormalizeOptionalValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
