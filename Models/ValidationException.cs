namespace BlockedCountriesApi.Models;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}

public class CountryAlreadyBlockedException : ValidationException
{
    public CountryAlreadyBlockedException(string countryCode) 
        : base($"Country {countryCode} is already blocked")
    {
    }
}

public class CountryNotBlockedException : ValidationException
{
    public CountryNotBlockedException(string countryCode) 
        : base($"Country {countryCode} is not blocked")
    {
    }
} 