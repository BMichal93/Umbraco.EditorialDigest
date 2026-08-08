using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Services;

public interface IEditorialDigestDeliveryService
{
    Task<int> SendAsync(EditorialDigestConfig configuration, DateTime utcNow, IReadOnlyCollection<string>? recipients = null, CancellationToken cancellationToken = default);
}
