namespace BlockedCountriesApi.Models;

public class BlockedCountry
{
    public string CountryCode { get; set; } = string.Empty;
    public DateTime BlockedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsTemporary => ExpiresAt.HasValue;
} 
public class TemporalBlockRequest
{
    public string CountryCode { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
} 