using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using FastEndpoints;

namespace BlockedCountriesApi.Endpoints.Countries;

public class GetBlockedCountriesEndpoint : Endpoint<GetBlockedCountriesRequest, object>
{
    private readonly ICountryBlockingService _countryBlockingService;
    private readonly ILogger<GetBlockedCountriesEndpoint> _logger;

    public GetBlockedCountriesEndpoint(
        ICountryBlockingService countryBlockingService,
        ILogger<GetBlockedCountriesEndpoint> logger)
    {
        _countryBlockingService = countryBlockingService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/countries/blocked");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("admin"));
    }

    public override async Task HandleAsync(GetBlockedCountriesRequest req, CancellationToken ct)
    {
        try
        {
            var countries = await _countryBlockingService.GetBlockedCountriesAsync(req.Page, req.PageSize, req.SearchTerm);
            await SendAsync(countries, cancellation: ct);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while getting blocked countries");
            await SendAsync(new { error = ex.Message }, 400, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting blocked countries");
            await SendAsync(new { error = "Internal server error" }, 500, ct);
        }
    }
}

public class GetBlockedCountriesRequest
{
    [QueryParam]
    public int Page { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
} 