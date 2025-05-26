using System.Net.Http.Json;
using BlockedCountriesApi.Models;
using Microsoft.Extensions.Options;

namespace BlockedCountriesApi.Services;

public class GeoLocationService : IGeoLocationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeoLocationService> _logger;

    public GeoLocationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeoLocationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GeoLocationResponse> GetLocationFromIpAsync(string ipAddress)
    {
        try
        {
            //var apiKey = _configuration["IpApi:ApiKey"];
            var response = await _httpClient.GetFromJsonAsync<GeoLocationResponse>(
                $"https://ipapi.co/{ipAddress}/json");

            if (response == null)
            {
                throw new Exception("Failed to get location data");
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location for IP {IpAddress}", ipAddress);
            throw;
        }
    }

    public async Task<GeoLocationResponse> GetLocationFromCurrentIpAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        return await GetLocationFromIpAsync(ipAddress);
    }
} 