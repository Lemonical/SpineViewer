namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the persisted viewer theme preference.
/// </summary>
public enum ViewerTheme
{
    /// <summary>
    /// Follows the host operating system preference.
    /// </summary>
    FollowSystem,

    /// <summary>
    /// Forces the light theme.
    /// </summary>
    Light,

    /// <summary>
    /// Forces the dark theme.
    /// </summary>
    Dark,
}
