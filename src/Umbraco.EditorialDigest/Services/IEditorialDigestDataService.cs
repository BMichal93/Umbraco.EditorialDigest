using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Services;

public interface IEditorialDigestDataService
{
    EditorialDigestData Collect(EditorialDigestConfig configuration, DateTime utcNow);
}
