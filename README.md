# Blocked Countries API

A .NET Core Web API that manages blocked countries and validates IP addresses using third-party geolocation APIs.

## Features

- Block/unblock countries
- Temporary country blocking with automatic expiration
- IP address geolocation lookup
- Blocked attempts logging
- In-memory data storage
- Swagger documentation
- Rate limiting support (configurable)

## Prerequisites

- .NET 9.0 SDK (Preview)
- API key from [ipapi.co](https://ipapi.co/) or [IPGeolocation.io](https://ipgeolocation.io/)

## Project Structure

```
BlockedCountriesApi/
├── Controllers/         # API controllers
├── Models/             # Data models and DTOs
├── Services/           # Business logic services
├── Repositories/       # Data access layer
├── Configuration/      # Application configuration
└── Properties/         # Project properties
```

## Setup

1. Clone the repository
2. Update the API key in `appsettings.json`:
   ```json
   {
     "IpApi": {
       "ApiKey": "YOUR_API_KEY_HERE"
     }
   }
   ```
3. Configure rate limiting (optional) in `appsettings.json`:
   ```json
   {
     "RateLimiting": {
       "PermitLimit": 100,
       "WindowMinutes": 1
     }
   }
   ```
4. Run the application:
   ```bash
   dotnet run
   ```
5. Access Swagger documentation at `http://localhost:5146/swagger` or `https://localhost:7264/swagger`

## API Endpoints

### Countries

- `POST /api/countries/block` - Block a country
  - Request: `"US"` (2-letter ISO country code)
  - Response: 200 OK or 409 Conflict if already blocked

- `DELETE /api/countries/block/{countryCode}` - Unblock a country
  - Response: 200 OK or 404 Not Found

- `GET /api/countries/blocked` - Get all blocked countries
  - Response: Array of blocked country codes

- `POST /api/countries/temporal-block` - Temporarily block a country
  - Request:
    ```json
    {
      "countryCode": "US",
      "durationMinutes": 120
    }
    ```
  - Response: 200 OK or 409 Conflict

### IP

- `GET /api/ip/lookup?ipAddress={ip}` - Look up IP address location
  - Response: Country information for the IP

- `GET /api/ip/check-block` - Check if current IP is blocked
  - Response:
    ```json
    {
      "isBlocked": false,
      "country": "United States"
    }
    ```

### Logs

- `GET /api/logs/blocked-attempts` - Get blocked attempts log
  - Response: Array of blocked attempt records

## Error Handling

The API returns appropriate HTTP status codes:

- 200: Success
- 400: Bad Request (invalid input)
- 403: Forbidden (IP is blocked)
- 404: Not Found
- 409: Conflict (country already blocked)
- 429: Too Many Requests (rate limit exceeded)
- 500: Internal Server Error

## Configuration

### Rate Limiting

Rate limiting can be configured in `appsettings.json`:

```json
{
  "RateLimiting": {
    "PermitLimit": 100,      // Number of requests allowed
    "WindowMinutes": 1       // Time window in minutes
  }
}
```

### IP API Configuration

Configure the IP geolocation service in `appsettings.json`:

```json
{
  "IpApi": {
    "ApiKey": "YOUR_API_KEY_HERE",
    "BaseUrl": "https://api.ipapi.com/api/"
  }
}
```

## Notes

- All country codes must be 2-letter ISO codes (e.g., "US", "GB", "EG")
- Temporary blocks must be between 1 and 1440 minutes (24 hours)
- The API uses in-memory storage, so data is lost on application restart
- Expired temporary blocks are automatically removed every 5 minutes
- Rate limiting is optional and can be configured or disabled 
