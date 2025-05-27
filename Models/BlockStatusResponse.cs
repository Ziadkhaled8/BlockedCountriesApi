namespace BlockedCountriesApi.Models;

public class BlockStatusResponse
{
    public bool IsBlocked { get; set; }
    public string Country { get; set; } = string.Empty;
} 