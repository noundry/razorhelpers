using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RazorHelpers;

/// <summary>
/// Provides static methods for creating Razor-based IResult responses.
/// </summary>
public static class RazorResults
{
    /// <summary>
    /// Creates a RazorComponentResult from a RenderFragment that can be returned from minimal API endpoints.
    /// </summary>
    /// <param name="fragment">The RenderFragment to render.</param>
    /// <param name="statusCode">The HTTP status code (optional).</param>
    /// <param name="contentType">The content type (optional, defaults to "text/html; charset=utf-8").</param>
    /// <returns>A RazorComponentResult that can be returned from minimal API endpoints.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fragment is null.</exception>
    /// <example>
    /// <code>
    /// app.MapGet("/", () => RazorResults.Razor(myFragment));
    /// </code>
    /// </example>
    public static RazorComponentResult Razor(
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
    /// <param name="fragment">The RenderFragment accepting a model parameter.</param>
    /// <param name="model">The model to pass to the RenderFragment.</param>
    /// <param name="statusCode">The HTTP status code (optional).</param>
    /// <param name="contentType">The content type (optional, defaults to "text/html; charset=utf-8").</param>
    /// <returns>A RazorComponentResult that can be returned from minimal API endpoints.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fragment or model is null.</exception>
    /// <example>
    /// <code>
    /// RenderFragment&lt;User&gt; template = user => builder => { };
    /// app.MapGet("/user/{id}", (int id) =>
    ///     RazorResults.Razor(template, new User { Id = id, Name = "John" }));
    /// </code>
    /// </example>
    public static RazorComponentResult Razor<TModel>(
        RenderFragment<TModel> fragment,
        TModel model,
        int? statusCode = null,
        string? contentType = null)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentNullException.ThrowIfNull(model);

        return Razor(fragment(model), statusCode, contentType);
    }
}
