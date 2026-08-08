namespace Umbraco.EditorialDigest.Services;

public interface IEditorialDigestEmailRenderer
{
    Task<string> RenderAsync(EditorialDigestEmailModel model, CancellationToken cancellationToken = default);
}
