# ImageJCsharp Roadmap

This roadmap describes the long-term direction for ImageJCsharp as a C# native replacement for ImageJ.

The project should grow in stages. Each stage must produce usable software before the next stage expands scope.

## Guiding Principle

Feasibility comes first.

The project should not try to beat ImageJ in the first stage. The first objective is to replace the most common ImageJ workflows with reliable C# implementations. Once the core workflows are stable, the project can expand into plugin ecosystems, advanced formats, stack processing, and higher-level scientific workflows.

## Near-Term Goals

Near-term work focuses on a usable MVP.

Target: a Windows desktop application that can replace ImageJ for basic image viewing, ROI selection, measurement, and simple processing.

### Product Goals

- Provide a working WinForms application.
- Use .NET Framework 4.8 across the solution.
- Support Windows 10 as the default deployment environment.
- Keep the UI familiar to ImageJ 1.x users.
- Implement the most feasible core workflows first.
- Keep the image processing core separate from the UI.
- Build enough command structure that later plugin work will not require rewriting the MVP.

### Core Functional Goals

- Open common image files.
- Save processed images.
- View images with zoom and scroll.
- Draw rectangle ROI.
- Measure ROI and full image statistics.
- Show measurement results in a table.
- Run basic processing commands: invert, threshold, edge detection.
- Add tests for core image model, ROI, measurements, and processing algorithms.

### Engineering Goals

- Keep core code testable without WinForms.
- Avoid plugin complexity until the built-in command path is stable.
- Avoid advanced scientific file formats until the basic image pipeline works.
- Avoid broad menu parity until the first workflows are reliable.

## Mid-Term Goals

Mid-term work turns the MVP into a practical ImageJ replacement for common lab workflows.

Target: cover the majority of everyday ImageJ 1.x usage without Java plugin compatibility.

### Product Goals

- Expand ROI tools beyond rectangle.
- Add ROI Manager.
- Add measurement configuration.
- Improve results table operations and export.
- Add profile plotting and further histogram refinements.
- Add common ImageJ menu commands in File, Edit, Image, Process, Analyze, and Window.
- Support image stacks for common workflows.
- Add a minimal C# plugin SDK.

### Core Functional Goals

- Oval ROI.
- Line and polyline ROI.
- Polygon ROI.
- Basic freehand ROI.
- Move, resize, duplicate, and delete ROI.
- Set Scale and physical unit calibration.
- Plot Profile.
- Brightness and contrast display adjustment.
- Crop, duplicate, resize, rotate, flip.
- Gaussian blur, median filter, sharpen, smooth.
- Binary morphology: erode, dilate, open, close.
- Analyze Particles baseline implementation.
- Multi-page TIFF stack open and navigation.
- Z projection: max, mean, sum.

### Engineering Goals

- Introduce a command registry.
- Make built-in menu actions use the same command abstraction as future plugins.
- Define plugin metadata and loading structure.
- Add automated regression image fixtures.
- Add golden-output tests for important algorithms.
- Improve performance through better pixel buffer access before considering native libraries.

## Long-Term Goals

Long-term work turns ImageJCsharp into a C# scientific image analysis platform.

Target: a sustainable ImageJ-like ecosystem in C#, with enough capability to support advanced scientific users and third-party extension authors.

### Product Goals

- Reach broad ImageJ 1.x built-in feature parity where feasible.
- Provide a stable C# plugin ecosystem.
- Reimplement high-value ImageJ/Fiji plugins in C# over time.
- Support more scientific image formats.
- Support automation and batch processing.
- Support advanced stack, time-series, and multi-channel workflows.

### Core Functional Goals

- C# plugin SDK with stable public interfaces.
- External plugin discovery and assembly loading.
- Plugin commands, tools, importers, exporters, measurements, and processors.
- Batch processing pipeline.
- Script automation through a C#-friendly scripting option.
- OME-TIFF and selected scientific microscopy formats.
- Advanced stack and hyperstack support.
- Image registration and stitching workflows.
- Tracking and segmentation workflows.
- Optional integration with optimized native libraries where it clearly pays off.

### Engineering Goals

- Stable API versioning for plugins.
- Plugin isolation strategy.
- Backward compatibility policy for plugin authors.
- Performance benchmark suite.
- Larger regression test corpus.
- Installer and update strategy.
- Documentation for users and plugin developers.

## Non-Goals

These are not project goals unless the strategy changes later:

- Loading Java ImageJ plugins directly.
- Full Fiji compatibility.
- Perfect reproduction of every historical ImageJ behavior in early stages.
- NativeAOT-first deployment.
- Becoming a general-purpose photo editor.

## Stage Exit Criteria

### Near-Term Exit Criteria

- A user can open an image, zoom, select a rectangle ROI, measure it, process it, and save the result.
- Core model and basic algorithms have automated tests.
- The app builds cleanly on .NET Framework 4.8.
- README and planning docs accurately describe current scope.

### Mid-Term Exit Criteria

- A user can complete common ImageJ workflows involving ROI Manager, histograms, calibration, binary processing, and simple stacks.
- Command registry is used by built-in features.
- A minimal external C# plugin can add a menu command.
- Core algorithms have regression tests with image fixtures.

### Long-Term Exit Criteria

- Third-party developers can build useful plugins against documented SDK APIs.
- Common advanced scientific workflows are possible without Java ImageJ.
- The project has a repeatable release, installer, and compatibility process.
