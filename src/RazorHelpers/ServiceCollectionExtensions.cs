using Microsoft.Extensions.DependencyInjection;

namespace RazorHelpers;

/// <summary>
/// Provides extension methods for configuring RazorHelpers services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RazorHelpers services to the service collection, including Razor Components support.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Services.AddRazorHelpers();
    /// </code>
    /// </example>
    public static IServiceCollection AddRazorHelpers(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRazorComponents();
        return services;
    }
}
