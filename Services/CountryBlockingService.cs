using System.Collections.Concurrent;
using System.Globalization;
using BlockedCountriesApi.Models;
using BlockedCountriesApi.Repositories;

namespace BlockedCountriesApi.Services;

public class CountryBlockingService : ICountryBlockingService
{
    private readonly IBlockedCountriesRepository _blockedCountriesRepository;
    private readonly IBlockedAttemptsRepository _blockedAttemptsRepository;

    public CountryBlockingService(
        IBlockedCountriesRepository blockedCountriesRepository,
        IBlockedAttemptsRepository blockedAttemptsRepository)
    {
        _blockedCountriesRepository = blockedCountriesRepository;
        _blockedAttemptsRepository = blockedAttemptsRepository;
    }

    private void ValidateCountryCode(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            throw new ValidationException("Country code cannot be empty");
        }

        if (countryCode.Length != 2)
        {
            throw new ValidationException("Country code must be 2 characters long");
        }
    }

    public async Task BlockCountryAsync(string countryCode)
    {
        ValidateCountryCode(countryCode);
        countryCode = countryCode.ToUpperInvariant();

        if (!await _blockedCountriesRepository.AddAsync(countryCode, new BlockedCountry
        {
            CountryCode = countryCode,
            BlockedAt = DateTime.UtcNow
        }))
        {
            throw new CountryAlreadyBlockedException(countryCode);
        }
    }

    public async Task UnblockCountryAsync(string countryCode)
    {
        ValidateCountryCode(countryCode);
        countryCode = countryCode.ToUpperInvariant();

        if (!await _blockedCountriesRepository.RemoveAsync(countryCode))
        {
            throw new CountryNotBlockedException(countryCode);
        }
    }

    public async Task<bool> IsCountryBlockedAsync(string countryCode)
    {
        ValidateCountryCode(countryCode);
        countryCode = countryCode.ToUpperInvariant();

        var blockedCountry = await _blockedCountriesRepository.GetAsync(countryCode);
        if (blockedCountry == null)
        {
            return false;
        }

        if (blockedCountry.ExpiresAt.HasValue && blockedCountry.ExpiresAt.Value < DateTime.UtcNow)
        {
            await _blockedCountriesRepository.RemoveAsync(countryCode);
            return false;
        }

        return true;
    }

    public async Task<IEnumerable<BlockedCountry>> GetBlockedCountriesAsync(int page = 1, int pageSize = 10, string? searchTerm = null)
    {
        if (page < 1)
        {
            throw new ValidationException("Page number must be greater than 0");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ValidationException("Page size must be between 1 and 100");
        }

        return await _blockedCountriesRepository.GetAllAsync((page - 1) * pageSize, pageSize, searchTerm);
    }

    public async Task TemporarilyBlockCountryAsync(string countryCode, int durationMinutes)
    {
        ValidateCountryCode(countryCode);

        if (durationMinutes < 1 || durationMinutes > 1440)
        {
            throw new ValidationException("Duration must be between 1 and 1440 minutes (24 hours)");
        }

        countryCode = countryCode.ToUpperInvariant();
        if (!await _blockedCountriesRepository.AddAsync(countryCode, new BlockedCountry
        {
            CountryCode = countryCode,
            BlockedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes)
        }))
        {
            throw new CountryAlreadyBlockedException(countryCode);
        }
    }

    public async Task LogBlockedAttemptAsync(BlockedAttempt attempt)
    {
        if (attempt == null)
        {
            throw new ValidationException("Attempt cannot be null");
        }

        if (string.IsNullOrWhiteSpace(attempt.IpAddress))
        {
            throw new ValidationException("IP address cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(attempt.CountryCode))
        {
            throw new ValidationException("Country code cannot be empty");
        }

        await _blockedAttemptsRepository.AddAsync(attempt);
    }

    public async Task<IEnumerable<BlockedAttempt>> GetBlockedAttemptsAsync(int page = 1, int pageSize = 10)
    {
        if (page < 1)
        {
            throw new ValidationException("Page number must be greater than 0");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ValidationException("Page size must be between 1 and 100");
        }

        return await _blockedAttemptsRepository.GetAllAsync((page - 1) * pageSize, pageSize);
    }

    public async Task<BlockStatusResponse> CheckIpBlockStatusAsync(GeoLocationResponse location, string userAgent)
    {
        if (location == null)
        {
            throw new ValidationException("Location information cannot be null");
        }

        if (string.IsNullOrWhiteSpace(location.Location.CountryCode2))
        {
            throw new ValidationException("Country code cannot be empty");
        }

        var isBlocked = await IsCountryBlockedAsync(location.Location.CountryCode2);

        var attempt = new BlockedAttempt
        {
            IpAddress = location.Ip,
            CountryCode = location.Location.CountryCode2,
            Timestamp = DateTime.UtcNow,
            WasBlocked = isBlocked,
            UserAgent = userAgent
        };

        await LogBlockedAttemptAsync(attempt);

        return new BlockStatusResponse
        {
            IsBlocked = isBlocked,
            Country = location.Location.CountryName
        };
    }
} 