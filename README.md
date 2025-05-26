# Blocked Countries API

A .NET Core Web API that manages blocked countries and validates IP addresses using third-party geolocation APIs.

## Features

- Block/unblock countries
- Temporary country blocking with automatic expiration
- IP address geolocation lookup
- Blocked attempts logging
- In-memory data storage
- Swagger documentation

## Prerequisites

- .NET 9.0 SDK
- API key from [ipapi.co](https://ipapi.co/) or [IPGeolocation.io](https://ipgeolocation.io/)

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
3. Run the application:
   ```bash
   dotnet run
   ```
4. Access Swagger documentation at `https://localhost:5001/swagger`

## API Endpoints

### Countries

- `POST /api/countries/block` - Block a country
- `DELETE /api/countries/block/{countryCode}` - Unblock a country
- `GET /api/countries/blocked` - Get all blocked countries
- `POST /api/countries/temporal-block` - Temporarily block a country

### IP

- `GET /api/ip/lookup?ipAddress={ip}` - Look up IP address location
- `GET /api/ip/check-block` - Check if current IP is blocked

### Logs

- `GET /api/logs/blocked-attempts` - Get blocked attempts log

## Request/Response Examples

### Block a Country

```http
POST /api/countries/block
Content-Type: application/json

"US"
```

### Temporarily Block a Country

```http
POST /api/countries/temporal-block
Content-Type: application/json

{
  "countryCode": "US",
  "durationMinutes": 120
}
```

### Check IP Block Status

```http
GET /api/ip/check-block
```

Response:
```json
{
  "isBlocked": false,
  "country": "United States"
}
```

## Error Handling

The API returns appropriate HTTP status codes:

- 200: Success
- 400: Bad Request (invalid input)
- 403: Forbidden (IP is blocked)
- 404: Not Found
- 409: Conflict (country already blocked)
- 500: Internal Server Error

## Notes

- All country codes must be 2-letter ISO codes (e.g., "US", "GB", "EG")
- Temporary blocks must be between 1 and 1440 minutes (24 hours)
- The API uses in-memory storage, so data is lost on application restart
- Expired temporary blocks are automatically removed every 5 minutes 