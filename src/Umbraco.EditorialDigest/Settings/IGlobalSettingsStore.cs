using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Settings;

public interface IGlobalSettingsStore
{
    EditorialDigestGlobalSettings GetCurrent();

    void Save(GlobalSettingsRequest request);
}
