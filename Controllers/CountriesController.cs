using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
        if (string.IsNullOrWhiteSpace(request.CountryCode) || request.CountryCode.Length != 2)
        {
            return BadRequest("Invalid country code");
        }

        if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
        {
            return BadRequest("Duration must be between 1 and 1440 minutes");
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
}

