using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.EditorialDigest.Constants;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Controllers;

[ApiController]
[Route("umbraco/management/api/v1/editorial-digest/settings")]
[Authorize(Roles = Umbraco.Cms.Core.Constants.Security.AdminGroupAlias)]
public sealed class GlobalSettingsApiController : ManagementApiControllerBase
{
    private readonly IGlobalSettingsStore _settingsStore;

    public GlobalSettingsApiController(IGlobalSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    [HttpGet]
    public ActionResult<GlobalSettingsRequest> GetSettings()
    {
        var settings = _settingsStore.GetCurrent();
        return Ok(new GlobalSettingsRequest
        {
            DefaultFromName = settings.DefaultFromName,
            DefaultFromEmail = settings.DefaultFromEmail,
            LogoUrl = settings.LogoUrl,
            CustomTemplateBasePath = settings.CustomTemplateBasePath,
            DashboardRefreshMinutes = settings.DashboardRefreshMinutes,
            IsPackageEnabled = settings.IsPackageEnabled,
            LoggingLevel = settings.LoggingLevel
        });
    }

    [HttpPut]
    public IActionResult Save([FromBody] GlobalSettingsRequest request)
    {
        var validationErrors = GlobalSettingsValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            foreach (var (field, errors) in validationErrors)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(field, error);
                }
            }

            return ValidationProblem(ModelState);
        }

        _settingsStore.Save(request);
        return NoContent();
    }
}
