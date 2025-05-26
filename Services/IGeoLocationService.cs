using BlockedCountriesApi.Models;

namespace BlockedCountriesApi.Services;

public interface IGeoLocationService
{
    Task<GeoLocationResponse> GetLocationFromIpAsync(string ipAddress);
    Task<GeoLocationResponse> GetLocationFromCurrentIpAsync(HttpContext context);
} 