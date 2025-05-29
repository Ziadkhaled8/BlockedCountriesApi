using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using FastEndpoints;

namespace BlockedCountriesApi.Endpoints.Ip;

public class CheckIpBlockStatusEndpoint : EndpointWithoutRequest<object>
{
    private readonly ICountryBlockingService _countryBlockingService;
    private readonly IGeoLocationService _geoLocationService;
    private readonly ILogger<CheckIpBlockStatusEndpoint> _logger;

    public CheckIpBlockStatusEndpoint(
        ICountryBlockingService countryBlockingService,
        IGeoLocationService geoLocationService,
        ILogger<CheckIpBlockStatusEndpoint> logger)
    {
        _countryBlockingService = countryBlockingService;
        _geoLocationService = geoLocationService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/ip/check");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("block-check"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            var geoLocation = await _geoLocationService.GetLocationFromCurrentIpAsync(HttpContext);
            var result = await _countryBlockingService.CheckIpBlockStatusAsync(geoLocation, userAgent);
            await SendAsync(result, cancellation: ct);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while checking IP block status");
            await SendAsync(new { error = ex.Message }, 400, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking IP block status");
            await SendAsync(new { error = "Internal server error" }, 500, ct);
        }
    }
} 