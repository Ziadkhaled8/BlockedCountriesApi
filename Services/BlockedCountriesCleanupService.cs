using BlockedCountriesApi.Models;
using BlockedCountriesApi.Repositories;

namespace BlockedCountriesApi.Services;

public class BlockedCountriesCleanupService : BackgroundService
{
    private readonly ILogger<BlockedCountriesCleanupService> _logger;
    private readonly IBlockedCountriesRepository _blockedCountriesRepository;

    public BlockedCountriesCleanupService(
        ILogger<BlockedCountriesCleanupService> logger,
        IBlockedCountriesRepository blockedCountriesRepository)
    {
        _logger = logger;
        _blockedCountriesRepository = blockedCountriesRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var allCountries = await _blockedCountriesRepository.GetAllAsync();
                var expiredCountries = allCountries
                    .Where(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value < DateTime.UtcNow)
                    .ToList();

                foreach (var country in expiredCountries)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    await _blockedCountriesRepository.RemoveAsync(country.CountryCode);
                    _logger.LogInformation("Removed expired temporary block for country {CountryCode}", country.CountryCode);
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