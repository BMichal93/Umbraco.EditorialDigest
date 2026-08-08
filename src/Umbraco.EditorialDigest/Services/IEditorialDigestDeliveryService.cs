using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Services;

public interface IEditorialDigestDeliveryService
{
    Task<int> SendAsync(EditorialDigestConfig configuration, DateTime utcNow, CancellationToken cancellationToken = default);
}
