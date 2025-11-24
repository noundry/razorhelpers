using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections;

namespace RazorHelpers;

/// <summary>
/// Internal component that wraps a RenderFragment for rendering.
/// </summary>
internal sealed class FragmentComponent : ComponentBase
{
    /// <summary>
    /// Gets or sets the RenderFragment to render.
    /// </summary>
    [Parameter]
    public required RenderFragment RenderFragment { get; set; }

    /// <summary>
    /// Builds the render tree by adding the RenderFragment content.
    /// </summary>
    /// <param name="builder">The RenderTreeBuilder to build the render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, RenderFragment);
    }

    /// <summary>
    /// A read-only dictionary that wraps a RenderFragment as a parameter dictionary.
    /// This is used internally to pass the RenderFragment as a component parameter.
    /// </summary>
    internal readonly struct ParametersDictionary : IReadOnlyDictionary<string, object?>, IDictionary<string, object?>
    {
        private readonly RenderFragment _renderFragment;

        /// <summary>
        /// Initializes a new instance of the ParametersDictionary struct.
        /// </summary>
        /// <param name="renderFragment">The RenderFragment to wrap.</param>
        public ParametersDictionary(RenderFragment renderFragment)
        {
            _renderFragment = renderFragment;
        }

        /// <summary>
        /// Gets the RenderFragment value for the specified key.
        /// </summary>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The RenderFragment if the key matches "RenderFragment".</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not "RenderFragment".</exception>
        public object? this[string key]
        {
            get => key == nameof(RenderFragment) ? _renderFragment : throw new KeyNotFoundException();
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a collection containing the single key "RenderFragment".
        /// </summary>
        public IEnumerable<string> Keys => [nameof(RenderFragment)];

        /// <summary>
        /// Gets a collection containing the RenderFragment value.
        /// </summary>
        public IEnumerable<object?> Values => throw new NotSupportedException();

        /// <summary>
        /// Gets the count of parameters (always 1).
        /// </summary>
        public int Count => 1;

        /// <summary>
        /// Gets a value indicating whether this dictionary is read-only (always true).
        /// </summary>
        public bool IsReadOnly => true;

        ICollection<string> IDictionary<string, object?>.Keys => throw new NotSupportedException();

        ICollection<object?> IDictionary<string, object?>.Values => throw new NotSupportedException();

        /// <summary>
        /// Determines whether this dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is "RenderFragment"; otherwise, false.</returns>
        public bool ContainsKey(string key) => key == nameof(RenderFragment);

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns>An enumerator for the dictionary.</returns>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            yield return new KeyValuePair<string, object?>(nameof(RenderFragment), _renderFragment);
        }

        /// <summary>
        /// Tries to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to retrieve.</param>
        /// <param name="value">The value associated with the key, if found.</param>
        /// <returns>True if the key is "RenderFragment"; otherwise, false.</returns>
        public bool TryGetValue(string key, out object? value)
        {
            if (key == nameof(RenderFragment))
            {
                value = _renderFragment;
                return true;
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void IDictionary<string, object?>.Add(string key, object? value) => throw new NotSupportedException();

        void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item) => throw new NotSupportedException();

        void ICollection<KeyValuePair<string, object?>>.Clear() => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item) =>
            item.Key == nameof(RenderFragment) && Equals(item.Value, _renderFragment);

        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) =>
            array[arrayIndex] = new KeyValuePair<string, object?>(nameof(RenderFragment), _renderFragment);

        bool IDictionary<string, object?>.Remove(string key) => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item) => throw new NotSupportedException();
    }
}
