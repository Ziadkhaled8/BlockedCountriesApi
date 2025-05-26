using BlockedCountriesApi.Configuration;
using BlockedCountriesApi.Services;

namespace BlockedCountriesApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure HttpClient
            builder.Services.AddHttpClient<IGeoLocationService, GeoLocationService>();

            // Register services
            builder.Services.AddSingleton<ICountryBlockingService, CountryBlockingService>();
            builder.Services.AddSingleton<IGeoLocationService, GeoLocationService>();
            builder.Services.AddHostedService<BlockedCountriesCleanupService>();


            builder.Services.ConfigureRateLimiting();

            var app = builder.Build();



            app.UseSwagger();
            app.UseSwaggerUI();
            

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Enable rate limiting
            app.UseRateLimiter();

            app.MapControllers();

            app.Run();
        }
    }
}
