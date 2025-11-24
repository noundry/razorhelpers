using Microsoft.Extensions.DependencyInjection;
using RazorHelpers;

namespace RazorHelpers.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRazorHelpers_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRazorHelpers();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddRazorHelpers_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddRazorHelpers());
    }

    [Fact]
    public void AddRazorHelpers_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddRazorHelpers();

        // Assert
        Assert.Same(services, result);
    }
}
