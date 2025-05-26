namespace BlockedCountriesApi.Models;

public class BlockedAttempt
{
    public string IpAddress { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool WasBlocked { get; set; }
    public string UserAgent { get; set; } = string.Empty;
} 