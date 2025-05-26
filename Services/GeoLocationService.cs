using System.Net;
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
            if (!IPAddress.TryParse(ipAddress, out _))
            {
                throw new ArgumentException("Invalid IP address format", nameof(ipAddress));
            }
            var apiKey = _configuration["IpApi:ApiKey"];


            var response = await _httpClient.GetFromJsonAsync<GeoLocationResponse>(
                $"https://api.ipgeolocation.io/v2/ipgeo?apiKey={apiKey}&ip={ipAddress}");

            return response ?? throw new Exception("Failed to get location data");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid IP address format: {IpAddress}", ipAddress);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location for IP {IpAddress}", ipAddress);
            throw;
        }
    }

    public async Task<GeoLocationResponse> GetLocationFromCurrentIpAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if(ipAddress == null || ipAddress=="::1" || ipAddress== "127.0.0.1")
            ipAddress = "8.8.8.8";
        return await GetLocationFromIpAsync(ipAddress);
    }
} 