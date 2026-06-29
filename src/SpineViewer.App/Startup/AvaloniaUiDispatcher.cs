using Avalonia;
using Avalonia.Threading;
using SpineViewer.Core.Abstractions;

namespace SpineViewer.App.Startup;

/// <summary>
/// Executes UI-bound callbacks through Avalonia's shared dispatcher.
/// </summary>
public sealed class AvaloniaUiDispatcher : IUiDispatcher
{
    /// <inheritdoc />
    public void Invoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (Application.Current is null)
        {
            callback();
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            callback();
            return;
        }

        Dispatcher.UIThread.InvokeAsync(callback).GetAwaiter().GetResult();
    }
}
