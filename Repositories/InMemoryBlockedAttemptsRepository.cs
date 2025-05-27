using BlockedCountriesApi.Models;
using System.Collections.Concurrent;

namespace BlockedCountriesApi.Repositories;

public class InMemoryBlockedAttemptsRepository : IBlockedAttemptsRepository
{
    private readonly ConcurrentBag<BlockedAttempt> _blockedAttempts = new();

    public Task AddAsync(BlockedAttempt attempt)
    {
        _blockedAttempts.Add(attempt);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<BlockedAttempt>> GetAllAsync(int skip, int take)
    {
        var attempts = _blockedAttempts
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take);

        return Task.FromResult(attempts);
    }
} 