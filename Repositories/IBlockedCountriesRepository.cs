using BlockedCountriesApi.Models;

namespace BlockedCountriesApi.Repositories;

public interface IBlockedCountriesRepository
{
    Task<bool> AddAsync(string countryCode, BlockedCountry country);
    Task<bool> RemoveAsync(string countryCode);
    Task<BlockedCountry?> GetAsync(string countryCode);
    Task<IEnumerable<BlockedCountry>> GetAllAsync(int skip, int take, string? searchTerm = null);
    Task<IEnumerable<BlockedCountry>> GetAllAsync();
} 