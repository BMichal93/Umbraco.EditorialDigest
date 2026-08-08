using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.EditorialDigest.Settings;

namespace Umbraco.EditorialDigest.Services;

public sealed class EditorialDigestScheduler : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private static readonly Action<ILogger, int, Exception?> LogDeliveryFailure = LoggerMessage.Define<int>(LogLevel.Error, new EventId(1, "EditorialDigestDeliveryFailed"), "Failed to send editorial digest {DigestId}");
    private static readonly Action<ILogger, Exception?> LogSchedulerUnavailable = LoggerMessage.Define(LogLevel.Warning, new EventId(2, "EditorialDigestSchedulerUnavailable"), "Editorial Digest scheduler is unavailable until package storage is ready");
    private readonly IGlobalSettingsStore _globalSettingsStore;
    private readonly IEditorialDigestConfigStore _configStore;
    private readonly IEditorialDigestDeliveryService _deliveryService;
    private readonly IEditorialDigestLogStore _logStore;
    private readonly ILogger<EditorialDigestScheduler> _logger;

    public EditorialDigestScheduler(IGlobalSettingsStore globalSettingsStore, IEditorialDigestConfigStore configStore, IEditorialDigestDeliveryService deliveryService, IEditorialDigestLogStore logStore, ILogger<EditorialDigestScheduler> logger)
    {
        _globalSettingsStore = globalSettingsStore;
        _configStore = configStore;
        _deliveryService = deliveryService;
        _logStore = logStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunDueDigestsAsync(DateTime.UtcNow, stoppingToken);
        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunDueDigestsAsync(DateTime.UtcNow, stoppingToken);
        }
    }

    internal async Task RunDueDigestsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        try
        {
            if (!_globalSettingsStore.GetCurrent().IsPackageEnabled)
            {
                return;
            }
        }
        catch (Exception exception)
        {
            LogSchedulerUnavailable(_logger, exception);
            return;
        }

        foreach (var configuration in _configStore.GetAll().Where(config => EditorialDigestSchedule.IsDue(config, utcNow)))
        {
            try
            {
                var recipientCount = await _deliveryService.SendAsync(configuration, utcNow, cancellationToken: cancellationToken);
                _configStore.SetRunResult(configuration.Id, utcNow, "Success", null, recipientCount);
                _logStore.Create(configuration.Id, utcNow, recipientCount, "Success", null, null);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                LogDeliveryFailure(_logger, configuration.Id, exception);
                _configStore.SetRunResult(configuration.Id, utcNow, "Failed", exception.Message, 0);
                _logStore.Create(configuration.Id, utcNow, 0, "Failed", exception.Message, null);
            }
        }
    }
}
