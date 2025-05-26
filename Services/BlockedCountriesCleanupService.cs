using System.Collections.Concurrent;
using BlockedCountriesApi.Models;

namespace BlockedCountriesApi.Services;

public class BlockedCountriesCleanupService : BackgroundService
{
    private readonly ILogger<BlockedCountriesCleanupService> _logger;
    private readonly IEnumerable<ConcurrentDictionary<string, BlockedCountry>> _blockedCountries;

    public BlockedCountriesCleanupService(
        ILogger<BlockedCountriesCleanupService> logger,
        ICountryBlockingService countryBlockingService)
    {
        _logger = logger;
        _blockedCountries = ((CountryBlockingService)countryBlockingService).GetBlockedCountries();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var expiredCountries = _blockedCountries
                    .Where(kvp => kvp.Value.ExpiresAt.HasValue && kvp.Value.ExpiresAt.Value < DateTime.UtcNow)
                    .ToList();

                foreach (var country in expiredCountries)
                {
                    _blockedCountries.TryRemove(country.Key, out _);
                    _logger.LogInformation("Removed expired temporary block for country {CountryCode}", country.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired blocks");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
} 