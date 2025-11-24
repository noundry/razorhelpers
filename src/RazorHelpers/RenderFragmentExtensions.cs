using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RazorHelpers;

/// <summary>
/// Provides extension methods for rendering RenderFragment instances to HTML strings.
/// </summary>
public static class RenderFragmentExtensions
{
    /// <summary>
    /// Renders a RenderFragment to an HTML string asynchronously.
    /// </summary>
    /// <param name="fragment">The RenderFragment to render.</param>
    /// <param name="services">The service provider containing required services (ILoggerFactory).</param>
    /// <returns>A task that represents the asynchronous render operation. The task result contains the rendered HTML string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fragment or services is null.</exception>
    /// <example>
    /// <code>
    /// var html = await myRenderFragment.RenderAsync(serviceProvider);
    /// </code>
    /// </example>
    public static async Task<string> RenderAsync(this RenderFragment fragment, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentNullException.ThrowIfNull(services);

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        using var renderer = new HtmlRenderer(services, loggerFactory);
        var parameters = new FragmentComponent.ParametersDictionary(fragment);
        var parameterView = ParameterView.FromDictionary(parameters);
        var root = await renderer.Dispatcher.InvokeAsync(() =>
            renderer.RenderComponentAsync<FragmentComponent>(parameterView));
        await root.QuiescenceTask;
        return await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
    }

    /// <summary>
    /// Renders a RenderFragment with a strongly-typed model to an HTML string asynchronously.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="fragment">The RenderFragment accepting a model parameter.</param>
    /// <param name="model">The model to pass to the RenderFragment.</param>
    /// <param name="services">The service provider containing required services (ILoggerFactory).</param>
    /// <returns>A task that represents the asynchronous render operation. The task result contains the rendered HTML string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fragment, model, or services is null.</exception>
    /// <example>
    /// <code>
    /// RenderFragment&lt;User&gt; template = (user) => @&lt;div&gt;@user.Name&lt;/div&gt;;
    /// var html = await template.RenderAsync(new User { Name = "John" }, serviceProvider);
    /// </code>
    /// </example>
    public static Task<string> RenderAsync<TModel>(
        this RenderFragment<TModel> fragment,
        TModel model,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(services);

        return fragment(model).RenderAsync(services);
    }
}
