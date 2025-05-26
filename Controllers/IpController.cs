using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BlockedCountriesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IpController : ControllerBase
{
    private readonly IGeoLocationService _geoLocationService;
    private readonly ICountryBlockingService _countryBlockingService;
    private readonly ILogger<IpController> _logger;

    public IpController(
        IGeoLocationService geoLocationService,
        ICountryBlockingService countryBlockingService,
        ILogger<IpController> logger)
    {
        _geoLocationService = geoLocationService;
        _countryBlockingService = countryBlockingService;
        _logger = logger;
    }

    [HttpGet("lookup")]
    [EnableRateLimiting("ip-lookup")]
    public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress)
    {
        try
        {
            var location = ipAddress == null
                ? await _geoLocationService.GetLocationFromCurrentIpAsync(HttpContext)
                : await _geoLocationService.GetLocationFromIpAsync(ipAddress);

            return Ok(location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up IP address");
            return StatusCode(500, "Error looking up IP address");
        }
    }

    [HttpGet("check-block")]
    [EnableRateLimiting("block-check")]
    public async Task<IActionResult> CheckBlock()
    {
        try
        {
            var location = await _geoLocationService.GetLocationFromCurrentIpAsync(HttpContext);
            var isBlocked = await _countryBlockingService.IsCountryBlockedAsync(location.CountryCode);

            var attempt = new BlockedAttempt
            {
                IpAddress = location.Ip,
                CountryCode = location.CountryCode,
                Timestamp = DateTime.UtcNow,
                WasBlocked = isBlocked,
                UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
            };

            await _countryBlockingService.LogBlockedAttemptAsync(attempt);

            if (isBlocked)
            {
                return Forbid();
            }

            return Ok(new { IsBlocked = false, Country = location.CountryName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if IP is blocked");
            return StatusCode(500, "Error checking if IP is blocked");
        }
    }
} 