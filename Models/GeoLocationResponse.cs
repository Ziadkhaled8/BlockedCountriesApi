namespace BlockedCountriesApi.Models;

public class GeoLocationResponse
{
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
} 