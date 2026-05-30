using SpineViewer.Features.Shell.ViewModels;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_SetsGenericDefaults()
    {
        MainWindowViewModel viewModel = new();

        Assert.Equal("SpineViewer", viewModel.Title);
        Assert.Equal("Welcome to SpineViewer.", viewModel.WelcomeMessage);
        Assert.Equal("Ready.", viewModel.StatusText);
    }
}
