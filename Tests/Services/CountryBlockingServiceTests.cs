using BlockedCountriesApi.Models;
using BlockedCountriesApi.Repositories;
using BlockedCountriesApi.Services;
using Moq;
using Xunit;

namespace BlockedCountriesApi.Tests.Services;

public class CountryBlockingServiceTests
{
    private readonly Mock<IBlockedCountriesRepository> _mockBlockedCountriesRepository;
    private readonly Mock<IBlockedAttemptsRepository> _mockBlockedAttemptsRepository;
    private readonly CountryBlockingService _service;

    public CountryBlockingServiceTests()
    {
        _mockBlockedCountriesRepository = new Mock<IBlockedCountriesRepository>();
        _mockBlockedAttemptsRepository = new Mock<IBlockedAttemptsRepository>();
        _service = new CountryBlockingService(
            _mockBlockedCountriesRepository.Object,
            _mockBlockedAttemptsRepository.Object);
    }

    [Fact]
    public async Task BlockCountryAsync_ValidCountryCode_ShouldAddCountry()
    {
        // Arrange
        var countryCode = "US";
        _mockBlockedCountriesRepository
            .Setup(r => r.AddAsync(countryCode, It.IsAny<BlockedCountry>()))
            .ReturnsAsync(true);

        // Act
        await _service.BlockCountryAsync(countryCode);

        // Assert
        _mockBlockedCountriesRepository.Verify(
            r => r.AddAsync(countryCode, It.Is<BlockedCountry>(c => 
                c.CountryCode == countryCode && 
                !c.ExpiresAt.HasValue)), 
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("USA")]
    [InlineData("U")]
    public async Task BlockCountryAsync_InvalidCountryCode_ShouldThrowValidationException(string countryCode)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _service.BlockCountryAsync(countryCode));
    }

    [Fact]
    public async Task BlockCountryAsync_AlreadyBlockedCountry_ShouldThrowCountryAlreadyBlockedException()
    {
        // Arrange
        var countryCode = "US";
        _mockBlockedCountriesRepository
            .Setup(r => r.AddAsync(countryCode, It.IsAny<BlockedCountry>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<CountryAlreadyBlockedException>(() => 
            _service.BlockCountryAsync(countryCode));
    }

    [Fact]
    public async Task UnblockCountryAsync_ValidCountryCode_ShouldRemoveCountry()
    {
        // Arrange
        var countryCode = "US";
        _mockBlockedCountriesRepository
            .Setup(r => r.RemoveAsync(countryCode))
            .ReturnsAsync(true);

        // Act
        await _service.UnblockCountryAsync(countryCode);

        // Assert
        _mockBlockedCountriesRepository.Verify(
            r => r.RemoveAsync(countryCode), 
            Times.Once);
    }

    [Fact]
    public async Task UnblockCountryAsync_NonBlockedCountry_ShouldThrowCountryNotBlockedException()
    {
        // Arrange
        var countryCode = "US";
        _mockBlockedCountriesRepository
            .Setup(r => r.RemoveAsync(countryCode))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<CountryNotBlockedException>(() => 
            _service.UnblockCountryAsync(countryCode));
    }

    [Fact]
    public async Task IsCountryBlockedAsync_BlockedCountry_ShouldReturnTrue()
    {
        // Arrange
        var countryCode = "US";
        var blockedCountry = new BlockedCountry
        {
            CountryCode = countryCode,
            BlockedAt = DateTime.UtcNow
        };

        _mockBlockedCountriesRepository
            .Setup(r => r.GetAsync(countryCode))
            .ReturnsAsync(blockedCountry);

        // Act
        var result = await _service.IsCountryBlockedAsync(countryCode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsCountryBlockedAsync_ExpiredBlock_ShouldReturnFalse()
    {
        // Arrange
        var countryCode = "US";
        var blockedCountry = new BlockedCountry
        {
            CountryCode = countryCode,
            BlockedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        _mockBlockedCountriesRepository
            .Setup(r => r.GetAsync(countryCode))
            .ReturnsAsync(blockedCountry);

        // Act
        var result = await _service.IsCountryBlockedAsync(countryCode);

        // Assert
        Assert.False(result);
        _mockBlockedCountriesRepository.Verify(
            r => r.RemoveAsync(countryCode), 
            Times.Once);
    }

    [Fact]
    public async Task TemporarilyBlockCountryAsync_ValidInput_ShouldAddTemporaryBlock()
    {
        // Arrange
        var countryCode = "US";
        var durationMinutes = 60;
        _mockBlockedCountriesRepository
            .Setup(r => r.AddAsync(countryCode, It.IsAny<BlockedCountry>()))
            .ReturnsAsync(true);

        // Act
        await _service.TemporarilyBlockCountryAsync(countryCode, durationMinutes);

        // Assert
        _mockBlockedCountriesRepository.Verify(
            r => r.AddAsync(countryCode, It.Is<BlockedCountry>(c => 
                c.CountryCode == countryCode && 
                c.ExpiresAt.HasValue)), 
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1441)]
    public async Task TemporarilyBlockCountryAsync_InvalidDuration_ShouldThrowValidationException(int durationMinutes)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _service.TemporarilyBlockCountryAsync("US", durationMinutes));
    }

    [Fact]
    public async Task CheckIpBlockStatusAsync_ValidInput_ShouldReturnCorrectStatus()
    {
        // Arrange
        var location = new GeoLocationResponse
        {
            Ip = "1.1.1.1",
            Location = new Location
            {
                CountryCode2 = "US",
                CountryName = "United States"
            }
        };
        var userAgent = "TestAgent";

        _mockBlockedCountriesRepository
            .Setup(r => r.GetAsync("US"))
            .ReturnsAsync(new BlockedCountry
            {
                CountryCode = "US",
                BlockedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.CheckIpBlockStatusAsync(location, userAgent);

        // Assert
        Assert.True(result.IsBlocked);
        Assert.Equal("United States", result.Country);
        _mockBlockedAttemptsRepository.Verify(
            r => r.AddAsync(It.Is<BlockedAttempt>(a => 
                a.IpAddress == location.Ip && 
                a.CountryCode == location.Location.CountryCode2 && 
                a.UserAgent == userAgent)), 
            Times.Once);
    }

    [Fact]
    public async Task CheckIpBlockStatusAsync_NullLocation_ShouldThrowValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _service.CheckIpBlockStatusAsync(null!, "TestAgent"));
    }

    [Fact]
    public async Task CheckIpBlockStatusAsync_EmptyCountryCode_ShouldThrowValidationException()
    {
        // Arrange
        var location = new GeoLocationResponse
        {
            Ip = "1.1.1.1",
            Location = new Location
            {
                CountryCode2 = "",
                CountryName = "United States"
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _service.CheckIpBlockStatusAsync(location, "TestAgent"));
    }
} 