using System.Collections.Concurrent;
using System.Globalization;
using BlockedCountriesApi.Models;

namespace BlockedCountriesApi.Services;

public class CountryBlockingService() : ICountryBlockingService
{
    private readonly ConcurrentDictionary<string, BlockedCountry> _blockedCountries = new();
    private readonly ConcurrentBag<BlockedAttempt> _blockedAttempts = new();

    public Task<bool> BlockCountryAsync(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
        {
            return Task.FromResult(false);
        }

        countryCode = countryCode.ToUpperInvariant();
        return Task.FromResult(_blockedCountries.TryAdd(countryCode, new BlockedCountry
        {
            CountryCode = countryCode,
            BlockedAt = DateTime.UtcNow
        }));
    }

    public Task<bool> UnblockCountryAsync(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
        {
            return Task.FromResult(false);
        }

        countryCode = countryCode.ToUpperInvariant();
        return Task.FromResult(_blockedCountries.TryRemove(countryCode, out _));
    }

    public Task<bool> IsCountryBlockedAsync(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
        {
            return Task.FromResult(false);
        }

        countryCode = countryCode.ToUpper();
        if (!_blockedCountries.TryGetValue(countryCode, out var blockedCountry))
        {
            return Task.FromResult(false);
        }

        if (blockedCountry.ExpiresAt.HasValue && blockedCountry.ExpiresAt.Value < DateTime.UtcNow)
        {
            _blockedCountries.TryRemove(countryCode, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public Task<IEnumerable<BlockedCountry>> GetBlockedCountriesAsync(int page = 1, int pageSize = 10, string? searchTerm = null)
    {
        var query = _blockedCountries.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToUpperInvariant();
            query = query.Where(c => c.CountryCode.Contains(searchTerm));
        }

        query = query.Skip((page - 1) * pageSize).Take(pageSize);
        return Task.FromResult(query);
    }

    public Task<bool> TemporarilyBlockCountryAsync(string countryCode, int durationMinutes)
    {
        
        countryCode = countryCode.ToUpperInvariant();
        if (_blockedCountries.ContainsKey(countryCode))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(_blockedCountries.TryAdd(countryCode, new BlockedCountry
        {
            CountryCode = countryCode,
            BlockedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes)
        }));
    }

    public Task LogBlockedAttemptAsync(BlockedAttempt attempt)
    {
        _blockedAttempts.Add(attempt);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<BlockedAttempt>> GetBlockedAttemptsAsync(int page = 1, int pageSize = 10)
    {
        var attempts = _blockedAttempts
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return Task.FromResult(attempts);
    }

    public Task<ConcurrentDictionary<string, BlockedCountry>> GetBlockedCountries()
    {
        return Task.FromResult(_blockedCountries);
    }
} 