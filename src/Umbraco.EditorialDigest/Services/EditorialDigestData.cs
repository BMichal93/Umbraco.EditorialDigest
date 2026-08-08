using Umbraco.EditorialDigest.Domain;

namespace Umbraco.EditorialDigest.Services;

public sealed record EditorialDigestData(DateTime GeneratedAtUtc, IReadOnlyCollection<EditorialDigestSectionData> Sections);

public sealed record EditorialDigestSectionData(DigestSection Section, IReadOnlyCollection<EditorialDigestItem> Items);

public sealed record EditorialDigestItem(int ContentId, string Name, string ContentTypeAlias, string AuthorName, DateTime DateUtc, string Status, string Context);
