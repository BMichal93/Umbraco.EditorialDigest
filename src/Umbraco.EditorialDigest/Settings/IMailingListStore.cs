using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Settings;

public interface IMailingListStore
{
    IReadOnlyCollection<EditorialDigestMailingListEntry> GetAll(int configId);
    EditorialDigestMailingListEntry? GetById(int id);
    int Create(int configId, MailingListEntryRequest request);
    bool Update(int id, MailingListEntryRequest request);
    bool Delete(int id);
}
