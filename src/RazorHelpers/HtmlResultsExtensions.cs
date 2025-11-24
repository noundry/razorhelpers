using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RazorHelpers;

/// <summary>
/// Provides extension methods for creating Razor-based IResult responses.
/// </summary>
public static class HtmlResultsExtensions
{
    /// <summary>
    /// Creates a RazorComponentResult from a RenderFragment that can be returned from minimal API endpoints.
    /// </summary>
    /// <param name="results">The IResultExtensions instance.</param>
    /// <param name="fragment">The RenderFragment to render.</param>
    /// <param name="statusCode">The HTTP status code (optional).</param>
    /// <param name="contentType">The content type (optional, defaults to "text/html; charset=utf-8").</param>
    /// <returns>A RazorComponentResult that can be returned from minimal API endpoints.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fragment is null.</exception>
    /// <example>
    /// <code>
    /// app.MapGet("/", () => Results.Razor(@&lt;h1&gt;Hello World&lt;/h1&gt;));
    /// </code>
    /// </example>
    public static RazorComponentResult Razor(
        this IResultExtensions results,
        RenderFragment fragment,
        int? statusCode = null,
        string? contentType = null)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        return new RazorComponentResult<FragmentComponent>(
            new FragmentComponent.ParametersDictionary(fragment))
        {
            StatusCode = statusCode,
            ContentType = contentType,
            PreventStreamingRendering = true
        };
    }

    /// <summary>
    /// Creates a RazorComponentResult from a RenderFragment with a strongly-typed model.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="results">The IResultExtensions instance.</param>
    /// <param name="fragment">The RenderFragment accepting a model parameter.</param>
    /// <param name="model">The model to pass to the RenderFragment.</param>
    /// <param name="statusCode">The HTTP status code (optional).</param>
    /// <param name="contentType">The content type (optional, defaults to "text/html; charset=utf-8").</param>
    /// <returns>A RazorComponentResult that can be returned from minimal API endpoints.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fragment or model is null.</exception>
    /// <example>
    /// <code>
    /// RenderFragment&lt;User&gt; template = (user) => @&lt;div&gt;@user.Name&lt;/div&gt;;
    /// app.MapGet("/user/{id}", (int id) =>
    ///     Results.Razor(template, new User { Id = id, Name = "John" }));
    /// </code>
    /// </example>
    public static RazorComponentResult Razor<TModel>(
        this IResultExtensions results,
        RenderFragment<TModel> fragment,
        TModel model,
        int? statusCode = null,
        string? contentType = null)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentNullException.ThrowIfNull(model);

        return results.Razor(fragment(model), statusCode, contentType);
    }
}
