using BlockedCountriesApi.Models;
using BlockedCountriesApi.Services;
using FastEndpoints;

namespace BlockedCountriesApi.Endpoints.Attempts;

public class GetBlockedAttemptsEndpoint : Endpoint<GetBlockedAttemptsRequest, object>
{
    private readonly ICountryBlockingService _countryBlockingService;
    private readonly ILogger<GetBlockedAttemptsEndpoint> _logger;

    public GetBlockedAttemptsEndpoint(
        ICountryBlockingService countryBlockingService,
        ILogger<GetBlockedAttemptsEndpoint> logger)
    {
        _countryBlockingService = countryBlockingService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/attempts");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("admin"));
    
    }

    public override async Task HandleAsync(GetBlockedAttemptsRequest req, CancellationToken ct)
    {
        try
        {
            var attempts = await _countryBlockingService.GetBlockedAttemptsAsync(req.Page, req.PageSize);
            await SendAsync(attempts, cancellation: ct);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while getting blocked attempts");
            await SendAsync(new { error = ex.Message }, 400, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting blocked attempts");
            await SendAsync(new { error = "Internal server error" }, 500, ct);
        }
    }
}

public class GetBlockedAttemptsRequest
{
    [QueryParam]
    public int Page { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;
} 