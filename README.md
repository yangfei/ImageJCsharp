# ImageJCsharp

ImageJCsharp aims to become a C# native replacement for ImageJ on Windows.

The long-term goal is not to build a generic image editor. The goal is to provide an ImageJ-like scientific image analysis application with familiar workflows, familiar menu organization, and a C#/.NET extension model that can grow into its own ecosystem.

## Long-Term Vision

ImageJ has been valuable for decades because it combines image viewing, analysis, measurement, batch processing, and extensibility in one practical tool. ImageJCsharp follows that spirit, but uses the C# ecosystem instead of the Java plugin ecosystem.

The long-term target is:

- Replace ImageJ for common scientific image analysis workflows.
- Keep the UI and menu structure close to ImageJ 1.x where practical.
- Implement the most used built-in ImageJ features in C#.
- Provide a stable C# plugin SDK for commands, tools, algorithms, importers, exporters, and analysis modules.
- Reimplement high-value ImageJ/Fiji ecosystem plugins in C# over time.
- Keep the image processing core independent from WinForms so algorithms can be tested, reused, and eventually automated.

See [ROADMAP.md](ROADMAP.md) for the staged near-term, mid-term, and long-term plan.
See [docs/near-term-plan.md](docs/near-term-plan.md) for the detailed near-term MVP checklist.

## Project Positioning

ImageJCsharp is intended to be:

- A scientific image processing and analysis application.
- A Windows desktop application.
- A C# native platform for future plugins.
- A practical ImageJ replacement for the most common 90% use cases before attempting full feature parity.

ImageJCsharp is not intended to be:

- A Java ImageJ plugin host.
- A Fiji compatibility layer.
- A Photoshop-like image editor.
- A NativeAOT-first application.
- A full clone of every historical ImageJ behavior in the first phase.

## Runtime Target

The project targets .NET Framework 4.8.

This is intentional:

- Windows 10 systems commonly support .NET Framework 4.8.
- WinForms support is mature and predictable.
- Dynamic loading, reflection, and future plugin support remain straightforward.
- The deployment model is simpler than NativeAOT for an extensible desktop application.

## MVP Goal

The first milestone is a feasible core replacement, not a perfect clone.

The MVP should cover the most common ImageJ workflows:

- Open and save common image formats.
- View, zoom, and pan images.
- Draw basic ROI selections.
- Measure area, mean, min, max, and standard deviation.
- Display and export measurement results.
- Adjust simple image data through core processing commands.
- Run thresholding and binary conversion.
- Run basic filters such as invert and edge detection.
- Establish the internal command architecture that future plugins can reuse.

## Initial MVP Scope

The first implementation focuses on features that are highly feasible and low risk.

### File

- Open PNG, JPEG, BMP, and TIFF.
- Save as PNG, JPEG, BMP, and TIFF.
- Close the current image.

### View

- Zoom in.
- Zoom out.
- Actual size.
- Fit to window.
- Pan through scrollbars.

### ROI

- Rectangle ROI.
- Move and resize support in later MVP iterations.
- Oval, line, polygon, and freehand ROI are planned after the rectangle workflow is stable.

### Analyze

- Measure current ROI.
- Measure full image when no ROI is selected.
- Results table.
- Pixel-based calibration first.
- Physical unit calibration later.

### Process

- Invert.
- Manual threshold.
- Binary image generation.
- Sobel edge detection.
- Blur, sharpen, median filter, morphology, and particle analysis after the first working shell.

## Explicitly Deferred

These are important, but they are not part of the first implementation target:

- Java plugin compatibility.
- ImageJ macro language compatibility.
- Fiji plugin compatibility.
- Bio-Formats full format support.
- Complex microscope vendor formats.
- Full stack and hyperstack support.
- 3D Viewer.
- TrackMate-like tracking.
- Stitching and registration workflows.
- GPU acceleration.
- Full undo/redo.
- Complete ImageJ menu parity.

## Architecture Direction

The project is split into a testable core library and a desktop shell.

```text
src/
  ImageJCsharp.Core/
    Image model, ROI model, measurements, processing algorithms.

  ImageJCsharp.App/
    WinForms UI, menus, dialogs, image display, command wiring.

tests/
  ImageJCsharp.Core.Tests/
    Unit tests for the core image model and algorithms.
```

The core library should not depend on WinForms. UI code may depend on the core library.

## Plugin Direction

The future plugin model should be C# native.

The planned direction is:

- Every menu action is internally represented as a command.
- Built-in features use the same command model as future plugins.
- External plugins are loaded from .NET assemblies.
- A plugin SDK exposes stable interfaces for image access, commands, tools, and analysis results.
- Algorithm plugins should avoid UI dependencies where possible.
- UI plugins may use WinForms when they need custom dialogs or panels.

## Development Principles

- Feasibility comes first.
- Implement stable core workflows before broad menu coverage.
- Prefer tested core behavior over UI-only behavior.
- Keep algorithms in the core library.
- Keep UI code focused on interaction and presentation.
- Avoid promising ImageJ compatibility that is not yet tested.
- Add features incrementally, with clear acceptance criteria.

## How to Help

Contributors are welcome.

Good first contribution areas include:

- Documentation improvements.
- Core unit tests.
- Simple image processing filters.
- ROI behavior.
- Measurement features.
- UI polish.
- Manual testing with real ImageJ workflows.
- Sample images for regression and smoke testing.

Please read [CONTRIBUTING.md](CONTRIBUTING.md) and [docs/contributor-guide.md](docs/contributor-guide.md) before starting.

## Release and Promotion

- Release process: [docs/release-process.md](docs/release-process.md)
- Promotion plan: [docs/promotion-plan.md](docs/promotion-plan.md)

## Current Status

This project is in the earliest MVP stage.

Implemented or in progress:

- Solution structure.
- .NET Framework 4.8 project targets.
- Core grayscale image model.
- Rectangle ROI model.
- Basic measurement model.
- Threshold, invert, and Sobel edge processing.
- Initial WinForms application shell.

## Name

`ImageJCsharp` is a working project name. It describes the technical goal clearly: an ImageJ-style application implemented in C#.
