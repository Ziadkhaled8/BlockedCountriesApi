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

    /// <summary>
    /// Blocks a country by its ISO 3166-1 alpha-2 code
    /// </summary>
    /// <param name="request">The country code to block</param>
    /// <returns>200 OK if successful, 400 Bad Request if invalid country code, 409 Conflict if country is already blocked</returns>
    [HttpPost("block")]
    public async Task<IActionResult> BlockCountry([FromBody] BlockCountryRequest request)
    {
        try
        {
            await _countryBlockingService.BlockCountryAsync(request.CountryCode);
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

    /// <summary>
    /// Unblocks a previously blocked country
    /// </summary>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code to unblock</param>
    /// <returns>200 OK if successful, 400 Bad Request if invalid country code, 404 Not Found if country is not blocked</returns>
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

    /// <summary>
    /// Gets a list of currently blocked countries
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page (1-100)</param>
    /// <param name="searchTerm">Optional search term to filter countries</param>
    /// <returns>List of blocked countries</returns>
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

    /// <summary>
    /// Temporarily blocks a country for a specified duration
    /// </summary>
    /// <param name="request">The country code and duration in minutes</param>
    /// <returns>200 OK if successful, 400 Bad Request if invalid parameters, 409 Conflict if country is already blocked</returns>
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

