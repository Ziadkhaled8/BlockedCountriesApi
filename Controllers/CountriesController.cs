using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;

namespace BlockedCountriesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("admin")]
public class CountriesController : ControllerBase
{
    private readonly ICountryBlockingService _countryBlockingService;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(
        ICountryBlockingService countryBlockingService,
        ILogger<CountriesController> logger)
    {
        _countryBlockingService = countryBlockingService;
        _logger = logger;
    }

    [HttpPost("block")]
    public async Task<IActionResult> BlockCountry([FromBody] string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
        {
            return BadRequest("Invalid country code");
        }

        var success = await _countryBlockingService.BlockCountryAsync(countryCode);
        if (!success)
        {
            return Conflict("Country is already blocked");
        }

        return Ok();
    }

    [HttpDelete("block/{countryCode}")]
    public async Task<IActionResult> UnblockCountry(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
        {
            return BadRequest("Invalid country code");
        }

        var success = await _countryBlockingService.UnblockCountryAsync(countryCode);
        if (!success)
        {
            return NotFound("Country is not blocked");
        }

        return Ok();
    }

    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlockedCountries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var countries = await _countryBlockingService.GetBlockedCountriesAsync(page, pageSize, searchTerm);
        return Ok(countries);
    }

    [HttpPost("temporal-block")]
    public async Task<IActionResult> TemporarilyBlockCountry([FromBody] TemporalBlockRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CountryCode) || request.CountryCode.Length != 2)
            {
                throw new ArgumentException("Invalid country code", nameof(request.CountryCode));
            }

            if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
            {
                throw new ArgumentOutOfRangeException(nameof(request.DurationMinutes), "Duration must be between 1 and 1440 minutes (24 hours).");
            }
            

            var success = await _countryBlockingService.TemporarilyBlockCountryAsync(
                request.CountryCode,
                request.DurationMinutes);

            if (!success)
            {
                return Conflict("Country is already blocked");
            }

            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input for temporarily blocking country");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while temporarily blocking country");
            return StatusCode(500, "Internal server error");
        }
    }
}

