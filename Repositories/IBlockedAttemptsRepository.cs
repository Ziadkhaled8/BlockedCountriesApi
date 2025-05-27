using BlockedCountriesApi.Models;

namespace BlockedCountriesApi.Repositories;

public interface IBlockedAttemptsRepository
{
    Task AddAsync(BlockedAttempt attempt);
    Task<IEnumerable<BlockedAttempt>> GetAllAsync(int skip, int take);
} 