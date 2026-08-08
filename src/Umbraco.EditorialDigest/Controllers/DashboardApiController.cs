using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.EditorialDigest.Constants;
using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Persistence;
using Umbraco.EditorialDigest.Services;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Controllers;

[PluginController(EditorialDigestConstants.AreaName)]
[Authorize]
public sealed class DashboardApiController : UmbracoAuthorizedJsonController
{
    private readonly IEditorialDigestDataService _dataService;
    private readonly IEditorialDigestConfigStore _configStore;
    private readonly IMailingListStore _mailingListStore;

    public DashboardApiController(IEditorialDigestDataService dataService, IEditorialDigestConfigStore configStore, IMailingListStore mailingListStore)
    {
        _dataService = dataService;
        _configStore = configStore;
        _mailingListStore = mailingListStore;
    }

    [HttpGet]
    public ActionResult<EditorialOverviewResponse> GetOverview()
    {
        var now = DateTime.UtcNow;
        var dashboardConfig = new EditorialDigestConfig
        {
            SectionsEnabled = "[0,3,5]",
            LookbackHours = 24,
            StaleDays = 90,
            MaxItemsPerSection = 10
        };
        var data = _dataService.Collect(dashboardConfig, now);
        var digests = _configStore.GetAll().Where(config => config.IsEnabled).Select(config => new ActiveDigestResponse(config.Id, config.Name, config.LastRunDate, config.LastRunStatus, _mailingListStore.GetAll(config.Id).Count(entry => entry.IsActive))).ToArray();
        return Ok(new EditorialOverviewResponse(now, data.Sections, digests));
    }
}

public sealed record EditorialOverviewResponse(DateTime GeneratedAtUtc, IReadOnlyCollection<EditorialDigestSectionData> Sections, IReadOnlyCollection<ActiveDigestResponse> ActiveDigests);
public sealed record ActiveDigestResponse(int Id, string Name, DateTime? LastRunDate, string? LastRunStatus, int RecipientCount);
