using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using FastEndpoints;

namespace BlockedCountriesApi.Endpoints.Countries;

public class UnblockCountryEndpoint : Endpoint<UnblockCountryRequest>
{
    private readonly ICountryBlockingService _countryBlockingService;
    private readonly ILogger<UnblockCountryEndpoint> _logger;

    public UnblockCountryEndpoint(
        ICountryBlockingService countryBlockingService,
        ILogger<UnblockCountryEndpoint> logger)
    {
        _countryBlockingService = countryBlockingService;
        _logger = logger;
    }

    public override void Configure()
    {
        Delete("/api/countries/block/{countryCode}");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("admin"));
    }

    public override async Task HandleAsync(UnblockCountryRequest req, CancellationToken ct)
    {
        try
        {
            await _countryBlockingService.UnblockCountryAsync(req.CountryCode);
            await SendOkAsync(ct);
        }
        catch (CountryNotBlockedException ex)
        {
            _logger.LogWarning(ex, "Attempt to unblock non-blocked country");
            await SendAsync(new { error = ex.Message }, 404, ct);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while unblocking country");
            await SendAsync(new { error = ex.Message }, 400, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while unblocking country");
            await SendAsync(new { error = "Internal server error" }, 500, ct);
        }
    }
}

public class UnblockCountryRequest
{
    public string CountryCode { get; set; } = string.Empty;
} 