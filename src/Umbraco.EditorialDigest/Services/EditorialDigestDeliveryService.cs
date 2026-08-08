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
    private readonly IEditorialDigestDataService _dataService;
    private readonly IEditorialDigestEmailRenderer _emailRenderer;
    private readonly IEmailSender _emailSender;

    public EditorialDigestDeliveryService(IGlobalSettingsStore globalSettingsStore, IMailingListStore mailingListStore, IUserService userService, IEditorialDigestDataService dataService, IEditorialDigestEmailRenderer emailRenderer, IEmailSender emailSender)
    {
        _globalSettingsStore = globalSettingsStore;
        _mailingListStore = mailingListStore;
        _userService = userService;
        _dataService = dataService;
        _emailRenderer = emailRenderer;
        _emailSender = emailSender;
    }

    public async Task<int> SendAsync(EditorialDigestConfig configuration, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var settings = _globalSettingsStore.GetCurrent();
        if (!settings.IsPackageEnabled || !configuration.IsEnabled || !_emailSender.CanSendRequiredEmail())
        {
            return 0;
        }

        var recipients = GetRecipients(configuration).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (recipients.Length == 0)
        {
            return 0;
        }

        var data = _dataService.Collect(configuration, utcNow);
        var model = new EditorialDigestEmailModel(configuration, settings, data, utcNow.AddHours(-configuration.LookbackHours), utcNow, "/umbraco");
        var body = await _emailRenderer.RenderAsync(model, cancellationToken);
        var from = string.IsNullOrWhiteSpace(configuration.FromEmail) ? settings.DefaultFromEmail : configuration.FromEmail;
        var subject = configuration.SubjectLineTemplate.Replace("{{digestName}}", configuration.Name, StringComparison.Ordinal).Replace("{{date}}", utcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), StringComparison.Ordinal);
        await _emailSender.SendAsync(new EmailMessage(from, recipients, null, null, string.IsNullOrWhiteSpace(configuration.ReplyToEmail) ? null : [configuration.ReplyToEmail], subject, body, true, null), "EditorialDigest");
        return recipients.Length;
    }

    private IEnumerable<string> GetRecipients(EditorialDigestConfig configuration)
    {
        if (configuration.RecipientSource is RecipientSource.CustomMailingList or RecipientSource.Both)
        {
            foreach (var entry in _mailingListStore.GetAll(configuration.Id).Where(entry => entry.IsActive)) yield return entry.Email;
        }

        if (configuration.RecipientSource is RecipientSource.UserGroups or RecipientSource.Both)
        {
            foreach (var alias in (configuration.RecipientUserGroups ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var group = _userService.GetUserGroupByAlias(alias);
                if (group is null) continue;
                foreach (var user in _userService.GetAllInGroup(group.Id).Where(user => !string.IsNullOrWhiteSpace(user.Email))) yield return user.Email!;
            }
        }
    }
}
