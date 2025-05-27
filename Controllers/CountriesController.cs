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
        try
        {
            await _countryBlockingService.BlockCountryAsync(countryCode);
            return Ok();
        }
        catch (CountryAlreadyBlockedException ex)
        {
            _logger.LogWarning(ex, "Attempt to block already blocked country");
            return Conflict(ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while blocking country");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while blocking country");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("block/{countryCode}")]
    public async Task<IActionResult> UnblockCountry(string countryCode)
    {
        try
        {
            await _countryBlockingService.UnblockCountryAsync(countryCode);
            return Ok();
        }
        catch (CountryNotBlockedException ex)
        {
            _logger.LogWarning(ex, "Attempt to unblock non-blocked country");
            return NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while unblocking country");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while unblocking country");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlockedCountries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var countries = await _countryBlockingService.GetBlockedCountriesAsync(page, pageSize, searchTerm);
            return Ok(countries);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while getting blocked countries");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting blocked countries");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("temporal-block")]
    public async Task<IActionResult> TemporarilyBlockCountry([FromBody] TemporalBlockRequest request)
    {
        try
        {
            await _countryBlockingService.TemporarilyBlockCountryAsync(
                request.CountryCode,
                request.DurationMinutes);

            return Ok();
        }
        catch (CountryAlreadyBlockedException ex)
        {
            _logger.LogWarning(ex, "Attempt to temporarily block already blocked country");
            return Conflict(ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while temporarily blocking country");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while temporarily blocking country");
            return StatusCode(500, "Internal server error");
        }
    }
}

