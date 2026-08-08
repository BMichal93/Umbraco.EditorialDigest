using System.Globalization;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Services;
using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Persistence;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Services;

public sealed class EditorialDigestDeliveryService : IEditorialDigestDeliveryService
{
    private readonly IGlobalSettingsStore _globalSettingsStore;
    private readonly IMailingListStore _mailingListStore;
    private readonly IUserService _userService;
    private readonly IUserGroupService _userGroupService;
    private readonly IEditorialDigestDataService _dataService;
    private readonly IEditorialDigestEmailRenderer _emailRenderer;
    private readonly IEmailSender _emailSender;

    public EditorialDigestDeliveryService(IGlobalSettingsStore globalSettingsStore, IMailingListStore mailingListStore, IUserService userService, IUserGroupService userGroupService, IEditorialDigestDataService dataService, IEditorialDigestEmailRenderer emailRenderer, IEmailSender emailSender)
    {
        _globalSettingsStore = globalSettingsStore;
        _mailingListStore = mailingListStore;
        _userService = userService;
        _userGroupService = userGroupService;
        _dataService = dataService;
        _emailRenderer = emailRenderer;
        _emailSender = emailSender;
    }

    public async Task<int> SendAsync(EditorialDigestConfig configuration, DateTime utcNow, IReadOnlyCollection<string>? recipients = null, CancellationToken cancellationToken = default)
    {
        var settings = _globalSettingsStore.GetCurrent();
        if (!settings.IsPackageEnabled || !configuration.IsEnabled || !_emailSender.CanSendRequiredEmail())
        {
            return 0;
        }

        var resolvedRecipients = (recipients ?? await GetRecipientsAsync(configuration)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (resolvedRecipients.Length == 0)
        {
            return 0;
        }

        var data = _dataService.Collect(configuration, utcNow);
        var model = new EditorialDigestEmailModel(configuration, settings, data, utcNow.AddHours(-configuration.LookbackHours), utcNow, "/umbraco");
        var body = await _emailRenderer.RenderAsync(model, cancellationToken);
        var from = string.IsNullOrWhiteSpace(configuration.FromEmail) ? settings.DefaultFromEmail : configuration.FromEmail;
        var subject = configuration.SubjectLineTemplate.Replace("{{digestName}}", configuration.Name, StringComparison.Ordinal).Replace("{{date}}", utcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), StringComparison.Ordinal);
        await _emailSender.SendAsync(new EmailMessage(from, resolvedRecipients, null, null, string.IsNullOrWhiteSpace(configuration.ReplyToEmail) ? null : [configuration.ReplyToEmail], subject, body, true, null), "EditorialDigest", true, null);
        return resolvedRecipients.Length;
    }

    private async Task<IReadOnlyCollection<string>> GetRecipientsAsync(EditorialDigestConfig configuration)
    {
        var recipients = new List<string>();
        if (configuration.RecipientSource is RecipientSource.CustomMailingList or RecipientSource.Both)
        {
            recipients.AddRange(_mailingListStore.GetAll(configuration.Id).Where(entry => entry.IsActive).Select(entry => entry.Email));
        }

        if (configuration.RecipientSource is RecipientSource.UserGroups or RecipientSource.Both)
        {
            foreach (var alias in (configuration.RecipientUserGroups ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var group = await _userGroupService.GetAsync(alias);
                if (group is null) continue;
                recipients.AddRange(_userService.GetAllInGroup(group.Id).Where(user => !string.IsNullOrWhiteSpace(user.Email)).Select(user => user.Email!));
            }
        }

        return recipients;
    }
}
