using BlockedCountriesApi.Configuration;
using BlockedCountriesApi.Repositories;
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
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Blocked Countries API",
                    Version = "v1",
                    Description = "API for managing blocked countries and checking IP block status",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "API Support",
                        Email = "ziadkhaledmadeeh@gmail.com"
                    }
                });
            });

            // Configure HttpClient
            builder.Services.AddHttpClient<IGeoLocationService, GeoLocationService>();

            // Register repositories
            builder.Services.AddSingleton<IBlockedCountriesRepository, InMemoryBlockedCountriesRepository>();
            builder.Services.AddSingleton<IBlockedAttemptsRepository, InMemoryBlockedAttemptsRepository>();

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
