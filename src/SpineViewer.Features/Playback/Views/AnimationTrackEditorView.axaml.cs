using Avalonia.Markup.Xaml;

namespace SpineViewer.Features.Playback.Views;

/// <summary>
/// Hosts the multi-track editor view.
/// </summary>
public partial class AnimationTrackEditorView : Avalonia.Controls.UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationTrackEditorView"/> class.
    /// </summary>
    public AnimationTrackEditorView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
