using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BlockedCountriesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("admin")]
public class LogsController : ControllerBase
{
    private readonly ICountryBlockingService _countryBlockingService;
    private readonly ILogger<LogsController> _logger;

    public LogsController(
        ICountryBlockingService countryBlockingService,
        ILogger<LogsController> logger)
    {
        _countryBlockingService = countryBlockingService;
        _logger = logger;
    }

    [HttpGet("blocked-attempts")]
    public async Task<IActionResult> GetBlockedAttempts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var attempts = await _countryBlockingService.GetBlockedAttemptsAsync(page, pageSize);
            return Ok(attempts);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while getting blocked attempts");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting blocked attempts");
            return StatusCode(500, "Internal server error");
        }
    }
} 