using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.EditorialDigest.Constants;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Controllers;

[PluginController(EditorialDigestConstants.AreaName)]
[Authorize(Roles = EditorialDigestConstants.AdministratorsGroupAlias)]
public sealed class GlobalSettingsApiController : UmbracoAuthorizedJsonController
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

    [HttpPost]
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
