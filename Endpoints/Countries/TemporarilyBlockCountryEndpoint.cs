using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using FastEndpoints;

namespace BlockedCountriesApi.Endpoints.Countries;

public class TemporarilyBlockCountryEndpoint : Endpoint<TemporalBlockRequest>
{
    private readonly ICountryBlockingService _countryBlockingService;
    private readonly ILogger<TemporarilyBlockCountryEndpoint> _logger;

    public TemporarilyBlockCountryEndpoint(
        ICountryBlockingService countryBlockingService,
        ILogger<TemporarilyBlockCountryEndpoint> logger)
    {
        _countryBlockingService = countryBlockingService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/countries/temporal-block");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("admin"));
    }

    public override async Task HandleAsync(TemporalBlockRequest req, CancellationToken ct)
    {
        try
        {
            await _countryBlockingService.TemporarilyBlockCountryAsync(req.CountryCode, req.DurationMinutes);
            await SendOkAsync(ct);
        }
        catch (CountryAlreadyBlockedException ex)
        {
            _logger.LogWarning(ex, "Attempt to temporarily block already blocked country");
            await SendAsync(new { error = ex.Message }, 409, ct);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while temporarily blocking country");
            await SendAsync(new { error = ex.Message }, 400, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while temporarily blocking country");
            await SendAsync(new { error = "Internal server error" }, 500, ct);
        }
    }
} 