namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Executes callbacks on the active application UI thread.
/// </summary>
public interface IUiDispatcher
{
    /// <summary>
    /// Executes the provided callback on the UI thread before returning.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    void Invoke(Action callback);
}
