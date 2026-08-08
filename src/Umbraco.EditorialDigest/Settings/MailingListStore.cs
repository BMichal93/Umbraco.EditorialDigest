using System.Globalization;
using System.Security.Cryptography;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Settings;

public sealed class MailingListStore : IMailingListStore
{
    private readonly IScopeProvider _scopeProvider;

    public MailingListStore(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public IReadOnlyCollection<EditorialDigestMailingListEntry> GetAll(int configId)
    {
        using var scope = _scopeProvider.CreateScope();
        return scope.Database.Fetch<EditorialDigestMailingListEntry>("WHERE ConfigId = @0 ORDER BY Email", configId);
    }

    public EditorialDigestMailingListEntry? GetById(int id)
    {
        using var scope = _scopeProvider.CreateScope();
        return scope.Database.SingleOrDefault<EditorialDigestMailingListEntry>("WHERE Id = @0", id);
    }

    public int Create(int configId, MailingListEntryRequest request)
    {
        using var scope = _scopeProvider.CreateScope();
        var now = DateTime.UtcNow;
        var entry = new EditorialDigestMailingListEntry
        {
            ConfigId = configId,
            UnsubscribeToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            CreatedDate = now
        };
        Apply(request, entry);
        var id = Convert.ToInt32(scope.Database.Insert(entry), CultureInfo.InvariantCulture);
        scope.Complete();
        return id;
    }

    public bool Update(int id, MailingListEntryRequest request)
    {
        using var scope = _scopeProvider.CreateScope();
        var entry = scope.Database.SingleOrDefault<EditorialDigestMailingListEntry>("WHERE Id = @0", id);
        if (entry is null)
        {
            return false;
        }

        Apply(request, entry);
        scope.Database.Update(entry);
        scope.Complete();
        return true;
    }

    public bool Delete(int id)
    {
        using var scope = _scopeProvider.CreateScope();
        var entry = scope.Database.SingleOrDefault<EditorialDigestMailingListEntry>("WHERE Id = @0", id);
        if (entry is null)
        {
            return false;
        }

        scope.Database.Delete(entry);
        scope.Complete();
        return true;
    }

    private static void Apply(MailingListEntryRequest request, EditorialDigestMailingListEntry entry)
    {
        entry.Email = request.Email.Trim();
        entry.Name = Normalize(request.Name);
        entry.Company = Normalize(request.Company);
        entry.IsActive = request.IsActive;
        entry.LastModifiedDate = DateTime.UtcNow;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
