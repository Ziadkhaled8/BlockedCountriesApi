using BlockedCountriesApi.Models;
using BlockedCountriesApi.Repositories;
using BlockedCountriesApi.Services;
using Moq;
using Xunit;

namespace BlockedCountriesApi.Tests.Services;

public class BlockedCountriesCleanupServiceTests
{
    private readonly Mock<IBlockedCountriesRepository> _mockBlockedCountriesRepository;
    private readonly Mock<ILogger<BlockedCountriesCleanupService>> _mockLogger;
    private readonly BlockedCountriesCleanupService _service;

    public BlockedCountriesCleanupServiceTests()
    {
        _mockBlockedCountriesRepository = new Mock<IBlockedCountriesRepository>();
        _mockLogger = new Mock<ILogger<BlockedCountriesCleanupService>>();
        _service = new BlockedCountriesCleanupService(
            _mockLogger.Object,
            _mockBlockedCountriesRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExpiredBlocks_ShouldRemoveThem()
    {
        // Arrange
        var expiredCountry = new BlockedCountry
        {
            CountryCode = "US",
            BlockedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        var activeCountry = new BlockedCountry
        {
            CountryCode = "GB",
            BlockedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockBlockedCountriesRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new[] { expiredCountry, activeCountry });

        // Act
        await _service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Give the service time to process
        await _service.StopAsync(CancellationToken.None);

        // Assert
        _mockBlockedCountriesRepository.Verify(
            r => r.RemoveAsync(expiredCountry.CountryCode),
            Times.Once);

        _mockBlockedCountriesRepository.Verify(
            r => r.RemoveAsync(activeCountry.CountryCode),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoExpiredBlocks_ShouldNotRemoveAny()
    {
        // Arrange
        var activeCountry = new BlockedCountry
        {
            CountryCode = "GB",
            BlockedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockBlockedCountriesRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new[] { activeCountry });

        // Act
        await _service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Give the service time to process
        await _service.StopAsync(CancellationToken.None);

        // Assert
        _mockBlockedCountriesRepository.Verify(
            r => r.RemoveAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldLogError()
    {
        // Arrange
        _mockBlockedCountriesRepository
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await _service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Give the service time to process
        await _service.StopAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error occurred while cleaning up expired blocks")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopProcessing()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var expiredCountry = new BlockedCountry
        {
            CountryCode = "US",
            BlockedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        _mockBlockedCountriesRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new[] { expiredCountry });

        // Act
        await _service.StartAsync(cts.Token);
        cts.Cancel();
        await Task.Delay(100); // Give the service time to process

        // Assert
        _mockBlockedCountriesRepository.Verify(
            r => r.RemoveAsync(It.IsAny<string>()),
            Times.Never);
    }
} 