using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Services;

public sealed record EditorialDigestEmailModel(
    EditorialDigestConfig Configuration,
    EditorialDigestGlobalSettings GlobalSettings,
    EditorialDigestData Data,
    DateTime RangeStartUtc,
    DateTime RangeEndUtc,
    string DashboardUrl);
