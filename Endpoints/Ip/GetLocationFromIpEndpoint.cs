using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using FromQueryAttribute = FastEndpoints.FromQueryAttribute;

namespace BlockedCountriesApi.Endpoints.Ip;

public class GetLocationFromIpEndpoint : Endpoint<GetLocationFromIpRequest, object>
{
    private readonly IGeoLocationService _geoLocationService;
    private readonly ILogger<GetLocationFromIpEndpoint> _logger;

    public GetLocationFromIpEndpoint(
        IGeoLocationService geoLocationService,
        ILogger<GetLocationFromIpEndpoint> logger)
    {
        _geoLocationService = geoLocationService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/ip/lookup");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("ip-lookup"));
    }


    public override async Task HandleAsync(GetLocationFromIpRequest req, CancellationToken ct)
    {
        try
        {
            var location = await _geoLocationService.GetLocationFromIpAsync(req.IpAddress);
            await SendAsync(location, cancellation: ct);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while getting location from IP");
            await SendAsync(new { error = ex.Message }, 400, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting location from IP");
            await SendAsync(new { error = "Internal server error" }, 500, ct);
        }
    }
}

public class GetLocationFromIpRequest
{
    [QueryParam]
    public string IpAddress { get; set; } = string.Empty;
} 