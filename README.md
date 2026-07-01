# SpineViewer

SpineViewer is an Avalonia-based application for opening and inspecting Spine projects. The primary desktop entrypoint lives under `src/SpineViewer.Desktop`, with a browser host under `src/SpineViewer.Browser`.

## Current Scope

- Open Spine projects through the rewrite-side workspace flow
- Restore recent files and last-session state
- Surface diagnostics and recovery guidance for load failures
- Host viewport, playback, inspector, track editor, and settings panes in the main shell
- Support Spine runtime adapters for `3.8.95` and `4.1.00`

## Prerequisites

- .NET 10 SDK

## Getting Started

Clone the repository and run the desktop host:

```bash
dotnet run --project src/SpineViewer.Desktop/SpineViewer.Desktop.csproj
```

## Repository Layout

```text
src/
  SpineViewer.App/            Shared application bootstrap and DI composition
  SpineViewer.Browser/        Browser host
  SpineViewer.Core/           Core models, contracts, and app services
  SpineViewer.Desktop/        Desktop entrypoint
  SpineViewer.Features/       Feature-scoped MVVM UI code
  SpineViewer.Infrastructure/ Persistence, runtime adapters, and integrations
tests/
  SpineViewer.*.Tests/        Unit and integration test projects
```

## Development Notes

- `src/SpineViewer.App` owns shared application bootstrap behavior, but the desktop process entrypoint is `src/SpineViewer.Desktop`.
- `src/SpineViewer.Browser` exists for the browser-hosted app surface and shares the same application layer.

## Related Projects

- [SpineViewerWPF](https://github.com/kiletw/SpineViewerWPF)
- [spine-runtimes](https://github.com/EsotericSoftware/spine-runtimes)
