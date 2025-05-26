using System.Text.Json.Serialization;

namespace BlockedCountriesApi.Models;

public class GeoLocationResponse
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;
    [JsonPropertyName("location")]
    public Location Location { get; set; } = new Location();
}

public class Location
{
    [JsonPropertyName("country_code2")]
    public string CountryCode2 { get; set; } = string.Empty;
    [JsonPropertyName("country_name")]
    public string CountryName { get; set; } = string.Empty;
}