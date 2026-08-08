using System.Text.Json;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Persistence;
using Umbraco.EditorialDigest.Services;

namespace Umbraco.EditorialDigest.Tests;

[TestFixture]
public sealed class EditorialDigestDataServiceTests
{
    [Test]
    public void CollectReturnsEnabledSectionsWithMostRecentItemsFirst()
    {
        var now = new DateTime(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc);
        var contentService = CreateContentService(
            CreateContent(1, "Older", true, now.AddHours(-3), now.AddDays(-10)),
            CreateContent(2, "Newer", true, now.AddHours(-1), now.AddDays(-5)),
            CreateContent(3, "Draft", false, null, now.AddHours(-2)),
            CreateContent(4, "Stale", true, now.AddDays(-100), now.AddDays(-100)));
        var userService = CreateUserService();
        var service = new EditorialDigestDataService(contentService.Object, userService.Object);

        var result = service.Collect(CreateConfiguration(DigestSection.RecentlyPublished, DigestSection.PendingReview, DigestSection.StaleContent), now);

        var sections = result.Sections.ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(sections.Select(section => section.Section), Is.EqualTo(new List<DigestSection> { DigestSection.RecentlyPublished, DigestSection.PendingReview, DigestSection.StaleContent }));
            Assert.That(sections[0].Items.Select(item => item.Name), Is.EqualTo(new List<string> { "Newer", "Older" }));
            Assert.That(sections[1].Items.Select(item => item.Name), Is.EqualTo(new List<string> { "Draft" }));
            Assert.That(sections[2].Items.Select(item => item.Name), Is.EqualTo(new List<string> { "Stale" }));
        });
    }

    [Test]
    public void CollectLimitsItemsPerSection()
    {
        var now = new DateTime(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc);
        var contentService = CreateContentService(
            CreateContent(1, "First", true, now.AddHours(-1), now),
            CreateContent(2, "Second", true, now.AddHours(-2), now));
        var service = new EditorialDigestDataService(contentService.Object, CreateUserService().Object);
        var configuration = CreateConfiguration(1, DigestSection.RecentlyPublished);

        var result = service.Collect(configuration, now);

        Assert.That(result.Sections.Single().Items.Select(item => item.Name), Is.EqualTo(new List<string> { "First" }));
    }

    private static Mock<IContentService> CreateContentService(params IContent[] content)
    {
        var service = new Mock<IContentService>();
        service.Setup(instance => instance.GetRootContent()).Returns(content);
        long total = 0;
        service.Setup(instance => instance.GetPagedDescendants(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out total, It.IsAny<IQuery<IContent>>(), It.IsAny<Ordering>())).Returns([]);
        return service;
    }

    private static Mock<IUserService> CreateUserService()
    {
        var user = new Mock<IUser>();
        user.SetupGet(instance => instance.Id).Returns(1);
        user.SetupGet(instance => instance.Name).Returns("Editor");
        user.SetupGet(instance => instance.Username).Returns("editor");

        var service = new Mock<IUserService>();
        service.Setup(instance => instance.GetUsersById(It.IsAny<int[]>())).Returns([user.Object]);
        return service;
    }

    private static IContent CreateContent(int id, string name, bool published, DateTime? publishDate, DateTime updateDate)
    {
        var contentType = new Mock<ISimpleContentType>();
        contentType.SetupGet(instance => instance.Alias).Returns("article");

        var content = new Mock<IContent>();
        content.SetupGet(instance => instance.Id).Returns(id);
        content.SetupGet(instance => instance.Name).Returns(name);
        content.SetupGet(instance => instance.ContentType).Returns(contentType.Object);
        content.SetupGet(instance => instance.WriterId).Returns(1);
        content.SetupGet(instance => instance.Published).Returns(published);
        content.SetupGet(instance => instance.PublishDate).Returns(publishDate);
        content.SetupGet(instance => instance.UpdateDate).Returns(updateDate);
        content.SetupGet(instance => instance.Trashed).Returns(false);
        return content.Object;
    }

    private static EditorialDigestConfig CreateConfiguration(params DigestSection[] sections)
        => CreateConfiguration(10, sections);

    private static EditorialDigestConfig CreateConfiguration(int maxItemsPerSection, params DigestSection[] sections)
        => new()
        {
            SectionsEnabled = JsonSerializer.Serialize(sections),
            LookbackHours = 24,
            StaleDays = 90,
            MaxItemsPerSection = maxItemsPerSection
        };
}
