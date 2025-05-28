using System.Collections.Concurrent;
using BlockedCountriesApi.Models;
using BlockedCountriesApi.Repositories;

namespace BlockedCountriesApi.Services;

public class CountryBlockingService : ICountryBlockingService
{
    private readonly IBlockedCountriesRepository _blockedCountriesRepository;
    private readonly IBlockedAttemptsRepository _blockedAttemptsRepository;

    private static readonly HashSet<string> ValidCountryCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AF", "AX", "AL", "DZ", "AS", "AD", "AO", "AI", "AQ", "AG", "AR", "AM", "AW", "AU", "AT", "AZ",
        "BS", "BH", "BD", "BB", "BY", "BE", "BZ", "BJ", "BM", "BT", "BO", "BQ", "BA", "BW", "BV", "BR",
        "IO", "BN", "BG", "BF", "BI", "KH", "CM", "CA", "CV", "KY", "CF", "TD", "CL", "CN", "CX", "CC",
        "CO", "KM", "CG", "CD", "CK", "CR", "CI", "HR", "CU", "CW", "CY", "CZ", "DK", "DJ", "DM", "DO",
        "EC", "EG", "SV", "GQ", "ER", "EE", "ET", "FK", "FO", "FJ", "FI", "FR", "GF", "PF", "TF", "GA",
        "GM", "GE", "DE", "GH", "GI", "GR", "GL", "GD", "GP", "GU", "GT", "GG", "GN", "GW", "GY", "HT",
        "HM", "VA", "HN", "HK", "HU", "IS", "IN", "ID", "IR", "IQ", "IE", "IM", "IL", "IT", "JM", "JP",
        "JE", "JO", "KZ", "KE", "KI", "KP", "KR", "KW", "KG", "LA", "LV", "LB", "LS", "LR", "LY", "LI",
        "LT", "LU", "MO", "MK", "MG", "MW", "MY", "MV", "ML", "MT", "MH", "MQ", "MR", "MU", "YT", "MX",
        "FM", "MD", "MC", "MN", "ME", "MS", "MA", "MZ", "MM", "NA", "NR", "NP", "NL", "NC", "NZ", "NI",
        "NE", "NG", "NU", "NF", "MP", "NO", "OM", "PK", "PW", "PS", "PA", "PG", "PY", "PE", "PH", "PN",
        "PL", "PT", "PR", "QA", "RE", "RO", "RU", "RW", "BL", "SH", "KN", "LC", "MF", "PM", "VC", "WS",
        "SM", "ST", "SA", "SN", "RS", "SC", "SL", "SG", "SX", "SK", "SI", "SB", "SO", "ZA", "GS", "SS",
        "ES", "LK", "SD", "SR", "SJ", "SZ", "SE", "CH", "SY", "TW", "TJ", "TZ", "TH", "TL", "TG", "TK",
        "TO", "TT", "TN", "TR", "TM", "TC", "TV", "UG", "UA", "AE", "GB", "US", "UM", "UY", "UZ", "VU",
        "VE", "VN", "VG", "VI", "WF", "EH", "YE", "ZM", "ZW"
    };

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

        // Check if the country code contains only letters
        if (!countryCode.All(char.IsLetter))
        {
            throw new ValidationException("Country code must contain only letters");
        }

        if (!ValidCountryCodes.Contains(countryCode.ToUpperInvariant()))
        {
            throw new ValidationException($"Invalid country code: {countryCode}. Must be a valid ISO 3166-1 alpha-2 code.");
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

    public async Task<ConcurrentDictionary<string, BlockedCountry>> GetBlockedCountries()
    {
        var countries = await _blockedCountriesRepository.GetAllAsync();
        return new ConcurrentDictionary<string, BlockedCountry>(
            countries.ToDictionary(c => c.CountryCode, c => c));
    }
} 