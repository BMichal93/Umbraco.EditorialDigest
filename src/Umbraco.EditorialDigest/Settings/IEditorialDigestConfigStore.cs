using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Settings;

public interface IEditorialDigestConfigStore
{
    IReadOnlyCollection<EditorialDigestConfig> GetAll();
    EditorialDigestConfig? GetById(int id);
    bool AliasExists(string configurationAlias, int? excludingId = null);
    int Create(DigestConfigRequest request);
    bool Update(int id, DigestConfigRequest request);
    bool Delete(int id);
    int? Duplicate(int id);
    void SetRunResult(int id, DateTime runDateUtc, string status, string? errorMessage, int recipientCount);
}
