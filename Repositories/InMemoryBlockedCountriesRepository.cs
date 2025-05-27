using BlockedCountriesApi.Models;
using System.Collections.Concurrent;

namespace BlockedCountriesApi.Repositories;

public class InMemoryBlockedCountriesRepository : IBlockedCountriesRepository
{
    private readonly ConcurrentDictionary<string, BlockedCountry> _blockedCountries = new();

    public Task<bool> AddAsync(string countryCode, BlockedCountry country)
    {
        return Task.FromResult(_blockedCountries.TryAdd(countryCode, country));
    }

    public Task<bool> RemoveAsync(string countryCode)
    {
        return Task.FromResult(_blockedCountries.TryRemove(countryCode, out _));
    }

    public Task<BlockedCountry?> GetAsync(string countryCode)
    {
        _blockedCountries.TryGetValue(countryCode, out var country);
        return Task.FromResult(country);
    }

    public Task<IEnumerable<BlockedCountry>> GetAllAsync(int skip, int take, string? searchTerm = null)
    {
        var query = _blockedCountries.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToUpperInvariant();
            query = query.Where(c => c.CountryCode.Contains(searchTerm));
        }

        query = query.Skip(skip).Take(take);
        return Task.FromResult(query);
    }

    public Task<IEnumerable<BlockedCountry>> GetAllAsync()
    {
        return Task.FromResult(_blockedCountries.Values.AsEnumerable());
    }
} 