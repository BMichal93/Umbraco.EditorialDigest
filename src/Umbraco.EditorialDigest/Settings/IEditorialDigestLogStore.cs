using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Settings;

public interface IEditorialDigestLogStore
{
    void Create(int configId, DateTime sentDateUtc, int recipientCount, string status, string? errorMessage, int? durationMs);
    IReadOnlyCollection<EditorialDigestLog> GetLatest(int configId, int count);
}
