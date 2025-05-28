using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using FastEndpoints;

namespace BlockedCountriesApi.Endpoints.Countries;

public class BlockCountryEndpoint : Endpoint<BlockCountryRequest>
{
    private readonly ICountryBlockingService _countryBlockingService;
    private readonly ILogger<BlockCountryEndpoint> _logger;

    public BlockCountryEndpoint(
        ICountryBlockingService countryBlockingService,
        ILogger<BlockCountryEndpoint> logger)
    {
        _countryBlockingService = countryBlockingService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/countries/block");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("admin"));
    }

    public override async Task HandleAsync(BlockCountryRequest req, CancellationToken ct)
    {
        try
        {
            await _countryBlockingService.BlockCountryAsync(req.CountryCode);
            await SendOkAsync(ct);
        }
        catch (CountryAlreadyBlockedException ex)
        {
            _logger.LogWarning(ex, "Attempt to block already blocked country");
            await SendAsync(new { error = ex.Message }, 409, ct);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while blocking country");
            await SendAsync(new { error = ex.Message }, 400, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while blocking country");
            await SendAsync(new { error = "Internal server error" }, 500, ct);
        }
    }
} 