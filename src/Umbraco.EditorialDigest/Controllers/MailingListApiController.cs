using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.EditorialDigest.Constants;
using Umbraco.EditorialDigest.Persistence;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Controllers;

[PluginController(EditorialDigestConstants.AreaName)]
[Authorize(Roles = Umbraco.Cms.Core.Constants.Security.AdminGroupAlias)]
public sealed class MailingListApiController : UmbracoAuthorizedJsonController
{
    private readonly IEditorialDigestConfigStore _configStore;
    private readonly IMailingListStore _mailingListStore;

    public MailingListApiController(IEditorialDigestConfigStore configStore, IMailingListStore mailingListStore)
    {
        _configStore = configStore;
        _mailingListStore = mailingListStore;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<MailingListEntryResponse>> GetAll(int configId)
    {
        if (_configStore.GetById(configId) is null)
        {
            return NotFound();
        }

        return Ok(_mailingListStore.GetAll(configId).Select(ToResponse));
    }

    [HttpPost]
    public ActionResult<MailingListEntryResponse> Create(int configId, [FromBody] MailingListEntryRequest request)
    {
        if (_configStore.GetById(configId) is null)
        {
            return NotFound();
        }

        if (!ValidateRequest(configId, request))
        {
            return ValidationProblem(ModelState);
        }

        var id = _mailingListStore.Create(configId, request);
        return Ok(ToResponse(_mailingListStore.GetById(id)!));
    }

    [HttpPost]
    public ActionResult<MailingListEntryResponse> Save(int configId, int id, [FromBody] MailingListEntryRequest request)
    {
        var entry = _mailingListStore.GetById(id);
        if (entry is null || entry.ConfigId != configId)
        {
            return NotFound();
        }

        if (!ValidateRequest(configId, request, id))
        {
            return ValidationProblem(ModelState);
        }

        _mailingListStore.Update(id, request);
        return Ok(ToResponse(_mailingListStore.GetById(id)!));
    }

    [HttpPost]
    public ActionResult<IReadOnlyCollection<MailingListEntryResponse>> Import(int configId, [FromBody] MailingListImportRequest request)
    {
        if (_configStore.GetById(configId) is null)
        {
            return NotFound();
        }

        var values = request.Values.Split([',', ';', '\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var existing = _mailingListStore.GetAll(configId).Select(entry => entry.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var imported = new List<MailingListEntryResponse>();

        foreach (var email in values)
        {
            var item = new MailingListEntryRequest { Email = email };
            if (!ValidateRequest(configId, item) || existing.Contains(email))
            {
                ModelState.Clear();
                continue;
            }

            var id = _mailingListStore.Create(configId, item);
            imported.Add(ToResponse(_mailingListStore.GetById(id)!));
            existing.Add(email);
        }

        return Ok(imported);
    }

    [HttpGet]
    public IActionResult Export(int configId)
    {
        if (_configStore.GetById(configId) is null)
        {
            return NotFound();
        }

        var lines = _mailingListStore.GetAll(configId)
            .Select(entry => string.Join(",", Escape(entry.Email), Escape(entry.Name), Escape(entry.Company), entry.IsActive));
        var csv = $"Email,Name,Company,IsActive{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"editorial-digest-recipients-{configId}.csv");
    }

    [HttpDelete]
    public IActionResult Delete(int configId, int id)
    {
        var entry = _mailingListStore.GetById(id);
        return entry is null || entry.ConfigId != configId
            ? NotFound()
            : _mailingListStore.Delete(id) ? NoContent() : NotFound();
    }

    private bool ValidateRequest(int configId, MailingListEntryRequest request, int? existingId = null)
    {
        foreach (var (field, errors) in MailingListValidator.Validate(request))
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(field, error);
            }
        }

        if (_mailingListStore.GetAll(configId).Any(entry => entry.Id != existingId && string.Equals(entry.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(request.Email), "Email is already on this mailing list.");
        }

        return ModelState.IsValid;
    }

    private static MailingListEntryResponse ToResponse(EditorialDigestMailingListEntry entry)
        => new()
        {
            Id = entry.Id,
            Email = entry.Email,
            Name = entry.Name,
            Company = entry.Company,
            IsActive = entry.IsActive
        };

    private static string Escape(string? value)
        => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
}
