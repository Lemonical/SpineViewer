namespace SpineViewer.Core.Services;

/// <summary>
/// Describes the current stage of a Spine project open/load workflow.
/// </summary>
public enum SpineLoadStage
{
    /// <summary>
    /// The workflow is resolving project files and their dependent assets.
    /// </summary>
    ResolvingProjectFiles,

    /// <summary>
    /// The workflow is detecting the most likely Spine export version.
    /// </summary>
    DetectingVersion,

    /// <summary>
    /// The workflow is selecting the runtime adapter to use.
    /// </summary>
    SelectingRuntime,

    /// <summary>
    /// The workflow is loading the project through the selected runtime.
    /// </summary>
    LoadingProject,

    /// <summary>
    /// The workflow has finished.
    /// </summary>
    Completed,
}
