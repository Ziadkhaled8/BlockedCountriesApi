using BlockedCountriesApi.Models;
using System.Collections.Concurrent;

namespace BlockedCountriesApi.Services;

public interface ICountryBlockingService
{
    Task BlockCountryAsync(string countryCode);
    Task UnblockCountryAsync(string countryCode);
    Task<bool> IsCountryBlockedAsync(string countryCode);
    Task<IEnumerable<BlockedCountry>> GetBlockedCountriesAsync(int page = 1, int pageSize = 10, string? searchTerm = null);
    Task<ConcurrentDictionary<string, BlockedCountry>> GetBlockedCountries();
    Task TemporarilyBlockCountryAsync(string countryCode, int durationMinutes);
    Task LogBlockedAttemptAsync(BlockedAttempt attempt);
    Task<IEnumerable<BlockedAttempt>> GetBlockedAttemptsAsync(int page = 1, int pageSize = 10);
    Task<BlockStatusResponse> CheckIpBlockStatusAsync(GeoLocationResponse location, string userAgent);
} 