using System.Globalization;
using System.Text.Json;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.EditorialDigest.Domain;
using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Settings;

public sealed class EditorialDigestConfigStore : IEditorialDigestConfigStore
{
    private readonly IScopeProvider _scopeProvider;

    public EditorialDigestConfigStore(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public IReadOnlyCollection<EditorialDigestConfig> GetAll()
    {
        using var scope = _scopeProvider.CreateScope();
        return scope.Database.Fetch<EditorialDigestConfig>("ORDER BY Name");
    }

    public EditorialDigestConfig? GetById(int id)
    {
        using var scope = _scopeProvider.CreateScope();
        return scope.Database.SingleOrDefault<EditorialDigestConfig>("WHERE Id = @0", id);
    }

    public bool AliasExists(string configurationAlias, int? excludingId = null)
    {
        using var scope = _scopeProvider.CreateScope();
        var config = scope.Database.FirstOrDefault<EditorialDigestConfig>("WHERE Alias = @0", configurationAlias);
        return config is not null && config.Id != excludingId;
    }

    public int Create(DigestConfigRequest request)
    {
        using var scope = _scopeProvider.CreateScope();
        var config = new EditorialDigestConfig { CreatedDate = DateTime.UtcNow };
        Apply(request, config);
        var id = Convert.ToInt32(scope.Database.Insert(config), CultureInfo.InvariantCulture);
        scope.Complete();
        return id;
    }

    public bool Update(int id, DigestConfigRequest request)
    {
        using var scope = _scopeProvider.CreateScope();
        var config = scope.Database.SingleOrDefault<EditorialDigestConfig>("WHERE Id = @0", id);
        if (config is null)
        {
            return false;
        }

        Apply(request, config);
        scope.Database.Update(config);
        scope.Complete();
        return true;
    }

    public bool Delete(int id)
    {
        using var scope = _scopeProvider.CreateScope();
        var config = scope.Database.SingleOrDefault<EditorialDigestConfig>("WHERE Id = @0", id);
        if (config is null)
        {
            return false;
        }

        scope.Database.Delete(config);
        scope.Complete();
        return true;
    }

    public int? Duplicate(int id)
    {
        using var scope = _scopeProvider.CreateScope();
        var source = scope.Database.SingleOrDefault<EditorialDigestConfig>("WHERE Id = @0", id);
        if (source is null)
        {
            return null;
        }

        var copy = new EditorialDigestConfig
        {
            Name = $"{source.Name} copy",
            Alias = GetAvailableAlias(scope, source.Alias),
            Description = source.Description,
            IsEnabled = false,
            RecipientSource = source.RecipientSource,
            RecipientUserGroups = source.RecipientUserGroups,
            ScheduleType = source.ScheduleType,
            ScheduleDay = source.ScheduleDay,
            ScheduleTime = source.ScheduleTime,
            TimeZoneId = source.TimeZoneId,
            SectionsEnabled = source.SectionsEnabled,
            LookbackHours = source.LookbackHours,
            UpcomingHours = source.UpcomingHours,
            StaleDays = source.StaleDays,
            ExpiringDays = source.ExpiringDays,
            MaxItemsPerSection = source.MaxItemsPerSection,
            SubjectLineTemplate = source.SubjectLineTemplate,
            FromName = source.FromName,
            FromEmail = source.FromEmail,
            ReplyToEmail = source.ReplyToEmail,
            CustomTemplatePath = source.CustomTemplatePath,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        var duplicatedId = Convert.ToInt32(scope.Database.Insert(copy), CultureInfo.InvariantCulture);
        scope.Complete();
        return duplicatedId;
    }

    private static void Apply(DigestConfigRequest request, EditorialDigestConfig config)
    {
        config.Name = request.Name.Trim();
        config.Alias = request.Alias.Trim();
        config.Description = Normalize(request.Description);
        config.IsEnabled = request.IsEnabled;
        config.RecipientSource = request.RecipientSource;
        config.RecipientUserGroups = Normalize(request.RecipientUserGroups);
        config.ScheduleType = request.ScheduleType;
        config.ScheduleDay = request.ScheduleType == ScheduleType.Weekly ? request.ScheduleDay : null;
        config.ScheduleTime = request.ScheduleTime;
        config.TimeZoneId = request.TimeZoneId.Trim();
        config.SectionsEnabled = JsonSerializer.Serialize(request.SectionsEnabled.Distinct().Order());
        config.LookbackHours = request.LookbackHours;
        config.UpcomingHours = request.UpcomingHours;
        config.StaleDays = request.StaleDays;
        config.ExpiringDays = request.ExpiringDays;
        config.MaxItemsPerSection = request.MaxItemsPerSection;
        config.SubjectLineTemplate = request.SubjectLineTemplate.Trim();
        config.FromName = Normalize(request.FromName);
        config.FromEmail = Normalize(request.FromEmail);
        config.ReplyToEmail = Normalize(request.ReplyToEmail);
        config.CustomTemplatePath = Normalize(request.CustomTemplatePath);
        config.LastModifiedDate = DateTime.UtcNow;
    }

    private static string GetAvailableAlias(IScope scope, string sourceAlias)
    {
        for (var suffix = 1; ; suffix++)
        {
            var candidate = $"{sourceAlias}-copy-{suffix}";
            if (scope.Database.FirstOrDefault<EditorialDigestConfig>("WHERE Alias = @0", candidate) is null)
            {
                return candidate;
            }
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
