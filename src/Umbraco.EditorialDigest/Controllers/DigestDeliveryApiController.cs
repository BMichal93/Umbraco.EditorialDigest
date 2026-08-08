using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.EditorialDigest.Constants;
using Umbraco.EditorialDigest.Services;
using Umbraco.EditorialDigest.Settings;
using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Controllers;

[ApiController]
[Route("umbraco/management/api/v1/editorial-digest/configurations/{id:int}/delivery")]
[Authorize(Roles = Umbraco.Cms.Core.Constants.Security.AdminGroupAlias)]
public sealed class DigestDeliveryApiController : ManagementApiControllerBase
{
    private readonly IEditorialDigestConfigStore _configStore;
    private readonly IEditorialDigestDeliveryService _deliveryService;
    private readonly IEditorialDigestLogStore _logStore;
    private readonly IGlobalSettingsStore _globalSettingsStore;
    private readonly IEditorialDigestDataService _dataService;
    private readonly IEditorialDigestEmailRenderer _emailRenderer;

    public DigestDeliveryApiController(IEditorialDigestConfigStore configStore, IEditorialDigestDeliveryService deliveryService, IEditorialDigestLogStore logStore, IGlobalSettingsStore globalSettingsStore, IEditorialDigestDataService dataService, IEditorialDigestEmailRenderer emailRenderer)
    {
        _configStore = configStore;
        _deliveryService = deliveryService;
        _logStore = logStore;
        _globalSettingsStore = globalSettingsStore;
        _dataService = dataService;
        _emailRenderer = emailRenderer;
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunNow(int id, CancellationToken cancellationToken)
    {
        var config = _configStore.GetById(id);
        if (config is null) return NotFound();

        try
        {
            var recipientCount = await _deliveryService.SendAsync(config, DateTime.UtcNow, cancellationToken: cancellationToken);
            _configStore.SetRunResult(id, DateTime.UtcNow, "Success", null, recipientCount);
            _logStore.Create(id, DateTime.UtcNow, recipientCount, "Success", null, null);
            return Ok(new { recipientCount });
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _configStore.SetRunResult(id, DateTime.UtcNow, "Failed", exception.Message, 0);
            _logStore.Create(id, DateTime.UtcNow, 0, "Failed", exception.Message, null);
            return Problem("The digest could not be sent.");
        }
    }

    [HttpGet("history")]
    public ActionResult<IReadOnlyCollection<DeliveryLogResponse>> GetHistory(int id)
        => Ok(_logStore.GetLatest(id, 10).Select(log => new DeliveryLogResponse(log.SentDate, log.RecipientCount, log.Status, log.ErrorMessage, log.DurationMs)));

    [HttpGet("preview")]
    public async Task<IActionResult> Preview(int id, CancellationToken cancellationToken)
    {
        var config = _configStore.GetById(id);
        if (config is null) return NotFound();
        var now = DateTime.UtcNow;
        var model = new EditorialDigestEmailModel(config, _globalSettingsStore.GetCurrent(), _dataService.Collect(config, now), now.AddHours(-config.LookbackHours), now, "/umbraco");
        var html = await _emailRenderer.RenderAsync(model, cancellationToken);
        return Content(html, "text/html");
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTestEmail(int id, [FromBody] TestEmailRequest request, CancellationToken cancellationToken)
    {
        if (!new EmailAddressAttribute().IsValid(request.Email)) return BadRequest(new { error = "A valid email address is required." });
        var config = _configStore.GetById(id);
        if (config is null) return NotFound();
        await _deliveryService.SendAsync(config, DateTime.UtcNow, [request.Email], cancellationToken);
        return Ok();
    }
}

public sealed record DeliveryLogResponse(DateTime SentDate, int RecipientCount, string Status, string? ErrorMessage, int? DurationMs);
public sealed record TestEmailRequest(string Email);
