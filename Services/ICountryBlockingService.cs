using BlockedCountriesApi.Models;
using System.Collections.Concurrent;

namespace BlockedCountriesApi.Services;

public interface ICountryBlockingService
{
    Task<bool> BlockCountryAsync(string countryCode);
    Task<bool> UnblockCountryAsync(string countryCode);
    Task<bool> IsCountryBlockedAsync(string countryCode);
    Task<IEnumerable<BlockedCountry>> GetBlockedCountriesAsync(int page = 1, int pageSize = 10, string? searchTerm = null);
    Task<IEnumerable<ConcurrentDictionary<string, BlockedCountry>>> GetBlockedCountries();
    Task<bool> TemporarilyBlockCountryAsync(string countryCode, int durationMinutes);
    Task LogBlockedAttemptAsync(BlockedAttempt attempt);
    Task<IEnumerable<BlockedAttempt>> GetBlockedAttemptsAsync(int page = 1, int pageSize = 10);
} 