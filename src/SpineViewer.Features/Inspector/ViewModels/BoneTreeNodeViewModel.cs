namespace SpineViewer.Features.Inspector.ViewModels;

/// <summary>
/// Represents one node in the inspected bones tree.
/// </summary>
public sealed class BoneTreeNodeViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoneTreeNodeViewModel"/> class.
    /// </summary>
    /// <param name="name">The bone name.</param>
    /// <param name="length">The inspected bone length.</param>
    /// <param name="children">The child bone nodes.</param>
    public BoneTreeNodeViewModel(
        string name,
        double length,
        IEnumerable<BoneTreeNodeViewModel> children)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        Name = name;
        Length = length < 0
            ? throw new ArgumentOutOfRangeException(nameof(length), "Value cannot be negative.")
            : length;
        Children = children?.ToArray() ?? throw new ArgumentNullException(nameof(children));
    }

    /// <summary>
    /// Gets the bone name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the inspected bone length.
    /// </summary>
    public double Length { get; }

    /// <summary>
    /// Gets the child bone nodes.
    /// </summary>
    public IReadOnlyList<BoneTreeNodeViewModel> Children { get; }

    /// <summary>
    /// Gets the concise label shown inside the tree.
    /// </summary>
    public string SummaryText => $"{Name} | {Length:0.##}";
}
