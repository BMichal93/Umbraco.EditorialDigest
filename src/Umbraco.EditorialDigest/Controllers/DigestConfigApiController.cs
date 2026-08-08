using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core.Services;
using Umbraco.EditorialDigest.Constants;
using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Persistence;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Controllers;

[ApiController]
[Route("umbraco/management/api/v1/editorial-digest/configurations")]
[Authorize(Roles = Umbraco.Cms.Core.Constants.Security.AdminGroupAlias)]
public sealed class DigestConfigApiController : ManagementApiControllerBase
{
    private readonly IEditorialDigestConfigStore _configStore;
    private readonly IUserService _userService;
    private readonly IUserGroupService _userGroupService;

    public DigestConfigApiController(IEditorialDigestConfigStore configStore, IUserService userService, IUserGroupService userGroupService)
    {
        _configStore = configStore;
        _userService = userService;
        _userGroupService = userGroupService;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<DigestConfigSummary>> GetAll()
        => Ok(_configStore.GetAll().Select(ToSummary));

    [HttpGet("{id:int}")]
    public ActionResult<DigestConfigResponse> Get(int id)
    {
        var config = _configStore.GetById(id);
        return config is null ? NotFound() : Ok(ToResponse(config));
    }

    [HttpGet("time-zones")]
    public ActionResult<IReadOnlyCollection<TimeZoneOption>> GetTimeZones()
        => Ok(TimeZoneInfo.GetSystemTimeZones().Select(timeZone => new TimeZoneOption(timeZone.Id, timeZone.DisplayName)));

    [HttpGet("user-groups")]
    public async Task<ActionResult<IReadOnlyCollection<UserGroupOption>>> GetUserGroups()
    {
        var groups = await _userGroupService.GetAllAsync(0, int.MaxValue);
        return Ok(groups.Items
            .OrderBy(group => group.Name)
            .Select(group => new UserGroupOption(group.Alias, group.Name ?? group.Alias))
            .ToArray());
    }

    [HttpPost]
    public ActionResult<DigestConfigResponse> Create([FromBody] DigestConfigRequest request)
    {
        if (!ValidateRequest(request))
        {
            return ValidationProblem(ModelState);
        }

        var id = _configStore.Create(request);
        return Ok(ToResponse(_configStore.GetById(id)!));
    }

    [HttpPut("{id:int}")]
    public ActionResult<DigestConfigResponse> Save(int id, [FromBody] DigestConfigRequest request)
    {
        if (!ValidateRequest(request, id))
        {
            return ValidationProblem(ModelState);
        }

        if (!_configStore.Update(id, request))
        {
            return NotFound();
        }

        return Ok(ToResponse(_configStore.GetById(id)!));
    }

    [HttpPost("{id:int}/duplicate")]
    public ActionResult<DigestConfigResponse> Duplicate(int id)
    {
        var duplicatedId = _configStore.Duplicate(id);
        return duplicatedId is null ? NotFound() : Ok(ToResponse(_configStore.GetById(duplicatedId.Value)!));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
        => _configStore.Delete(id) ? NoContent() : NotFound();

    private bool ValidateRequest(DigestConfigRequest request, int? existingId = null)
    {
        foreach (var (field, errors) in DigestConfigValidator.Validate(request))
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(field, error);
            }
        }

        if (_configStore.AliasExists(request.Alias.Trim(), existingId))
        {
            ModelState.AddModelError(nameof(request.Alias), "Alias is already in use.");
        }

        return ModelState.IsValid;
    }

    private static DigestConfigSummary ToSummary(EditorialDigestConfig config)
        => new()
        {
            Id = config.Id,
            Name = config.Name,
            Alias = config.Alias,
            IsEnabled = config.IsEnabled,
            LastRunDate = config.LastRunDate,
            LastRunStatus = config.LastRunStatus,
            LastRunRecipientCount = config.LastRunRecipientCount
        };

    private static DigestConfigResponse ToResponse(EditorialDigestConfig config)
        => new()
        {
            Id = config.Id,
            Name = config.Name,
            Alias = config.Alias,
            Description = config.Description,
            IsEnabled = config.IsEnabled,
            RecipientSource = config.RecipientSource,
            RecipientUserGroups = config.RecipientUserGroups,
            ScheduleType = config.ScheduleType,
            ScheduleDay = config.ScheduleDay,
            ScheduleTime = config.ScheduleTime,
            TimeZoneId = config.TimeZoneId,
            SectionsEnabled = DeserializeSections(config.SectionsEnabled),
            LookbackHours = config.LookbackHours,
            UpcomingHours = config.UpcomingHours,
            StaleDays = config.StaleDays,
            ExpiringDays = config.ExpiringDays,
            MaxItemsPerSection = config.MaxItemsPerSection,
            SubjectLineTemplate = config.SubjectLineTemplate,
            FromName = config.FromName,
            FromEmail = config.FromEmail,
            ReplyToEmail = config.ReplyToEmail,
            CustomTemplatePath = config.CustomTemplatePath,
            LastRunDate = config.LastRunDate,
            LastRunStatus = config.LastRunStatus,
            LastRunRecipientCount = config.LastRunRecipientCount
        };

    private static DigestSection[] DeserializeSections(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<DigestSection[]>(value) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

public sealed record TimeZoneOption(string Id, string DisplayName);

public sealed record UserGroupOption(string Alias, string Name);
