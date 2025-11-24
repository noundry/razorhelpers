using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RazorHelpers;

/// <summary>
/// Provides helper methods for rendering Razor components.
/// </summary>
public static class ComponentHelper
{
    /// <summary>
    /// Renders a component of the specified type to an HTML string.
    /// </summary>
    /// <typeparam name="TComponent">The type of component to render.</typeparam>
    /// <param name="services">The service provider containing required services.</param>
    /// <param name="parameters">Optional parameters to pass to the component.</param>
    /// <returns>A task that represents the asynchronous render operation. The task result contains the rendered HTML string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <example>
    /// <code>
    /// var html = await ComponentHelper.RenderComponentAsync&lt;MyComponent&gt;(
    ///     serviceProvider,
    ///     new Dictionary&lt;string, object?&gt; { ["Title"] = "Hello" });
    /// </code>
    /// </example>
    public static async Task<string> RenderComponentAsync<TComponent>(
        IServiceProvider services,
        IReadOnlyDictionary<string, object?>? parameters = null)
        where TComponent : IComponent
    {
        ArgumentNullException.ThrowIfNull(services);

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        using var renderer = new HtmlRenderer(services, loggerFactory);

        ParameterView parameterView = parameters != null
            ? ParameterView.FromDictionary(parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
            : ParameterView.Empty;

        var root = await renderer.Dispatcher.InvokeAsync(() =>
            renderer.RenderComponentAsync<TComponent>(parameterView));
        await root.QuiescenceTask;
        return await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
    }

    /// <summary>
    /// Renders a component of the specified type to an HTML string with a single parameter.
    /// </summary>
    /// <typeparam name="TComponent">The type of component to render.</typeparam>
    /// <typeparam name="TValue">The type of the parameter value.</typeparam>
    /// <param name="services">The service provider containing required services.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterValue">The value of the parameter.</param>
    /// <returns>A task that represents the asynchronous render operation. The task result contains the rendered HTML string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or parameterName is null.</exception>
    public static Task<string> RenderComponentAsync<TComponent, TValue>(
        IServiceProvider services,
        string parameterName,
        TValue parameterValue)
        where TComponent : IComponent
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(parameterName);

        var parameters = new Dictionary<string, object?> { [parameterName] = parameterValue };
        return RenderComponentAsync<TComponent>(services, parameters);
    }
}
