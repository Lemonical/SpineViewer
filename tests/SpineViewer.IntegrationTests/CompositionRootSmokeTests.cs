using System.Diagnostics;
using Xunit;

namespace SpineViewer.IntegrationTests;

public sealed class CompositionRootSmokeTests
{
    [Fact]
    public async Task SmokeTestCommand_ExitsSuccessfullyAsync()
    {
        await RunSmokeTestAsync();
    }

    [Fact]
    public async Task SmokeTestCommand_CanRunTwiceSequentiallyAsync()
    {
        await RunSmokeTestAsync();
        await RunSmokeTestAsync();
    }

    private static string GetRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            if (File.Exists(Path.Combine(currentDirectory.FullName, "PLAN.md")))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("The repository root could not be located from the test output directory.");
    }

    private static async Task RunSmokeTestAsync()
    {
        string repositoryRoot = GetRepositoryRoot();
        string projectPath = Path.Combine(repositoryRoot, "src", "SpineViewer.Desktop", "SpineViewer.Desktop.csproj");

        ProcessStartInfo startInfo = new("dotnet", $"run --project \"{projectPath}\" -- --smoke-test")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = repositoryRoot,
        };

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The smoke test process could not be started.");

        string standardOutput = await process.StandardOutput.ReadToEndAsync();
        string standardError = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        Assert.True(
            process.ExitCode == 0,
            $"Smoke test failed with exit code {process.ExitCode}.{Environment.NewLine}" +
            $"stdout:{Environment.NewLine}{standardOutput}{Environment.NewLine}" +
            $"stderr:{Environment.NewLine}{standardError}");
    }
}
