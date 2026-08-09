namespace Umbraco.EditorialDigest.MockUmbraco;

internal sealed record MockContentItem(string Name, string? ParentName, string Summary, bool IsPublished);

internal static class EditorialDigestMockContent
{
    public const string ContentTypeAlias = "editorialPage";

    public static readonly IReadOnlyList<MockContentItem> Items =
    [
        new("Editorial Home", null, "Mock root for Editorial Digest acceptance testing.", true),
        new("Recently Published Release Notes", "Editorial Home", "A published item for the recent activity section.", true),
        new("Content Standards", "Editorial Home", "A published editorial standards page.", true),
        new("Draft Pending Review", "Editorial Home", "A saved but unpublished page for review workflows.", false)
    ];
}
