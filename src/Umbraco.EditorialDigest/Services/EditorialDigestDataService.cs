using System.Text.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Services;

public sealed class EditorialDigestDataService : IEditorialDigestDataService
{
    private const int PageSize = 200;
    private readonly IContentService _contentService;
    private readonly IUserService _userService;

    public EditorialDigestDataService(IContentService contentService, IUserService userService)
    {
        _contentService = contentService;
        _userService = userService;
    }

    public EditorialDigestData Collect(EditorialDigestConfig configuration, DateTime utcNow)
    {
        var sections = GetEnabledSections(configuration);
        if (sections.Count == 0)
        {
            return new EditorialDigestData(utcNow, []);
        }

        var content = GetContent();
        var authors = GetAuthors(content);
        var result = new List<EditorialDigestSectionData>();

        if (sections.Contains(DigestSection.RecentlyPublished))
        {
            var since = utcNow.AddHours(-configuration.LookbackHours);
            result.Add(CreateSection(DigestSection.RecentlyPublished, content.Where(item => item.Published && item.PublishDate >= since), authors, configuration.MaxItemsPerSection, item => item.PublishDate!.Value, "Published"));
        }

        if (sections.Contains(DigestSection.PendingReview))
        {
            result.Add(CreateSection(DigestSection.PendingReview, content.Where(item => !item.Published), authors, configuration.MaxItemsPerSection, item => item.UpdateDate, "Pending review"));
        }

        if (sections.Contains(DigestSection.StaleContent))
        {
            var before = utcNow.AddDays(-configuration.StaleDays);
            result.Add(CreateSection(DigestSection.StaleContent, content.Where(item => item.Published && item.UpdateDate <= before), authors, configuration.MaxItemsPerSection, item => item.UpdateDate, "Stale"));
        }

        return new EditorialDigestData(utcNow, result);
    }

    private IContent[] GetContent()
    {
        var result = new List<IContent>();
        foreach (var root in _contentService.GetRootContent())
        {
            result.Add(root);
            for (var pageIndex = 0L; ; pageIndex++)
            {
                var children = _contentService.GetPagedDescendants(root.Id, pageIndex, PageSize, out var totalRecords).ToArray();
                result.AddRange(children);
                if ((pageIndex + 1) * PageSize >= totalRecords)
                {
                    break;
                }
            }
        }

        return result.Where(item => !item.Trashed).ToArray();
    }

    private Dictionary<int, string> GetAuthors(IEnumerable<IContent> content)
        => _userService.GetUsersById(content.Select(item => item.WriterId).Distinct().ToArray())
            .ToDictionary(user => user.Id, user => user.Name ?? user.Username);

    private static EditorialDigestSectionData CreateSection(DigestSection section, IEnumerable<IContent> content, IReadOnlyDictionary<int, string> authors, int limit, Func<IContent, DateTime> date, string status)
    {
        var items = content.OrderByDescending(date).Take(limit).Select(item =>
        {
            var itemDate = date(item);
            var author = authors.GetValueOrDefault(item.WriterId, "Unknown");
            return new EditorialDigestItem(item.Id, item.Name ?? "Untitled", item.ContentType.Alias, author, itemDate, status, $"{status} by {author} on {itemDate:yyyy-MM-dd HH:mm} UTC");
        }).ToArray();

        return new EditorialDigestSectionData(section, items);
    }

    private static HashSet<DigestSection> GetEnabledSections(EditorialDigestConfig configuration)
    {
        try
        {
            return JsonSerializer.Deserialize<HashSet<DigestSection>>(configuration.SectionsEnabled) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
