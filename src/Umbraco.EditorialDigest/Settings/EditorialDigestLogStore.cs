using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Settings;

public sealed class EditorialDigestLogStore : IEditorialDigestLogStore
{
    private readonly IScopeProvider _scopeProvider;

    public EditorialDigestLogStore(IScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

    public void Create(int configId, DateTime sentDateUtc, int recipientCount, string status, string? errorMessage, int? durationMs)
    {
        using var scope = _scopeProvider.CreateScope();
        scope.Database.Insert(new EditorialDigestLog { ConfigId = configId, SentDate = sentDateUtc, RecipientCount = recipientCount, Status = status, ErrorMessage = errorMessage, DurationMs = durationMs });
        scope.Complete();
    }

    public IReadOnlyCollection<EditorialDigestLog> GetLatest(int configId, int count)
    {
        using var scope = _scopeProvider.CreateScope();
        return scope.Database.Fetch<EditorialDigestLog>("WHERE ConfigId = @0 ORDER BY SentDate DESC", configId).Take(count).ToArray();
    }
}
