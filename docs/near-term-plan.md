# Near-Term Plan

This document lists the complete near-term target for ImageJCsharp.

The near-term plan is intentionally conservative. It includes only goals that are feasible with the current technology choice: .NET Framework 4.8, WinForms, and a C# core library.

## Near-Term Definition

Near-term means the first usable MVP.

The MVP is complete when a user can:

- Open a common image file.
- View it comfortably.
- Select a rectangle ROI.
- Measure the selected region.
- Apply a few basic processing commands.
- Save the processed image.
- Trust that core calculations are covered by automated tests.

## Technical Baseline

- Target framework: .NET Framework 4.8.
- UI: WinForms.
- Core library: `ImageJCsharp.Core`.
- Desktop app: `ImageJCsharp.App`.
- Tests: `ImageJCsharp.Core.Tests` and `ImageJCsharp.App.Tests`.
- Language: C#.
- Platform: Windows 10 or newer.

## Required Project Structure

- `src/ImageJCsharp.Core/`
  Core image data, ROI, measurement, and algorithm code.

- `src/ImageJCsharp.App/`
  WinForms UI, menus, dialogs, image display, and command wiring.

- `tests/ImageJCsharp.Core.Tests/`
  Automated tests for non-UI behavior.

- `tests/ImageJCsharp.App.Tests/`
  Focused tests for app-level helpers and startup-sensitive WinForms behavior.

- `README.md`
  Project identity, vision, MVP boundaries, and architecture direction.

- `ROADMAP.md`
  Near-term, mid-term, and long-term planning outline.

## Required MVP Features

### 1. Application Shell

The application must provide a basic desktop shell.

Required goals:

- Main application window.
- Menu bar.
- Status bar.
- Image display area.
- Measurement results table.
- Clean startup without requiring an opened image.
- Clean behavior when commands are used without an active image.

Minimum menu structure:

- File
- Edit
- Image
- Process
- Analyze
- View
- Window
- Help

For near-term, some menu entries may be disabled or absent if they are not implemented yet. Implemented commands must be reliable.

### 2. File Operations

The application must handle common image files.

Required goals:

- Open PNG.
- Open JPEG.
- Open BMP.
- Open basic TIFF.
- Save as PNG.
- Save as JPEG.
- Save as BMP.
- Save as TIFF.
- Close current image.
- Show current image name in the window title.

Deferred:

- Complex scientific TIFF metadata.
- OME-TIFF.
- Vendor-specific microscopy formats.
- Batch open.
- Recent files list.

### 3. Image Display

The application must provide basic viewing operations.

Required goals:

- Display loaded image.
- Zoom in.
- Zoom out.
- Actual size.
- Fit to window.
- Scroll/pan when image is larger than viewport.
- Show basic image size and zoom level in the status bar.

Deferred:

- Multi-document window management.
- Advanced LUT display.
- Channel display modes.
- Stack slice controls.

### 4. Core Image Model

The core library must provide a simple image representation.

Required goals:

- Grayscale image model.
- Width and height.
- Row-major pixel access.
- Pixel buffer copy.
- Safe bounds checking.

Near-term accepted limitation:

- Internally convert opened images to grayscale for MVP processing.

Deferred:

- Native RGB processing model.
- 16-bit scientific image preservation across all file formats.
- 32-bit float image model.
- Multi-channel image model.
- Stack and hyperstack model.

### 5. ROI

The application must support the first ROI workflow.

Required goals:

- Rectangle ROI creation by mouse drag.
- Rectangle ROI display overlay.
- Clamp ROI to image bounds.
- Measure full image when no ROI exists.

Deferred:

- ROI move.
- ROI Manager.
- Oval ROI.
- Line ROI.
- Polygon ROI.
- Freehand ROI.
- Overlay list.

### 6. Measurements

The core library and UI must support basic measurement.

Required goals:

- Pixel count.
- Area.
- Mean.
- Minimum.
- Maximum.
- Standard deviation.
- Results table row per measurement.

Required tests:

- Measurement inside a rectangle ROI.
- Measurement uses only pixels inside ROI.
- Measurement rejects ROI outside the image if there is no intersection.

Deferred:

- Measurement options dialog.
- Integrated density.
- Perimeter.
- Shape descriptors.
- Feret diameter.
- Centroid.
- Physical unit calibration UI.

### 7. Processing Commands

The MVP must include a few basic processing commands.

Required goals:

- Invert.
- Manual threshold.
- Convert threshold result to binary image.
- Sobel edge detection.

Required tests:

- Invert flips pixel values against a specified maximum.
- Threshold includes pixels inside the selected range.
- Sobel detects a simple vertical edge.

Deferred:

- Gaussian blur.
- Median filter.
- Smooth.
- Sharpen.
- Morphology.
- Watershed.
- Analyze Particles.
- FFT.

### 8. Command Direction

Near-term work should prepare for a command architecture without overbuilding the plugin system.

Required goals:

- Treat each menu action as a command conceptually.
- Keep command handlers small.
- Keep algorithm logic out of WinForms event handlers.
- Avoid plugin loading until built-in command paths are stable.

Deferred:

- External plugin discovery.
- Plugin metadata.
- Plugin SDK NuGet package.
- Assembly isolation.

### 9. Testing

Core behavior must be covered by automated tests.

Required goals:

- Tests compile and run under .NET Framework 4.8.
- Tests cover `GrayImage`.
- Tests cover rectangle ROI measurement.
- Tests cover threshold.
- Tests cover invert.
- Tests cover Sobel edge detection.

Required verification command:

```powershell
dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj
```

Expected result:

- Exit code 0.
- All tests pass.

### 10. Build Verification

The whole solution must build cleanly.

Required verification command:

```powershell
dotnet build ImageJCsharp.sln
```

Expected result:

- Exit code 0.
- 0 errors.

Near-term preference:

- Keep warnings at 0 where practical.

## Near-Term Completion Checklist

This checklist tracks implemented MVP capabilities. Full manual smoke validation and common-format open/save documentation are tracked separately.

- [x] README states the project vision clearly.
- [x] ROADMAP defines near-term, mid-term, and long-term goals.
- [x] App, core, and test projects target .NET Framework 4.8.
- [x] The solution builds.
- [x] Core tests pass.
- [x] App starts without crashing.
- [x] User can open PNG.
- [x] User can open JPEG.
- [x] User can open BMP.
- [x] User can open basic TIFF.
- [x] User can save PNG.
- [x] User can save JPEG.
- [x] User can save BMP.
- [x] User can save TIFF.
- [x] User can zoom in.
- [x] User can zoom out.
- [x] User can return to actual size.
- [x] User can fit image to window.
- [x] User can draw rectangle ROI.
- [x] User can resize rectangle ROI.
- [x] User can measure rectangle ROI.
- [x] User can measure full image with no ROI.
- [x] Results table shows measurement rows.
- [x] User can export measurement results to CSV.
- [x] User can view a histogram for the full image or rectangle ROI.
- [x] User can apply invert.
- [x] User can apply manual threshold.
- [x] User can apply Sobel edge detection.
- [x] Core image operations are not implemented inside WinForms event handlers.

## Known Near-Term Tradeoffs

- Grayscale-first is accepted for feasibility.
- Single-document UI is accepted for the first usable shell.
- Rectangle ROI only is accepted until measurement flow is stable.
- Basic TIFF support through platform image codecs is accepted.
- UI tests are deferred; core tests are required.
- Plugin loading is deferred; command-oriented structure is still required.

## Near-Term Exit Criteria

The near-term plan is complete when:

- `dotnet build ImageJCsharp.sln` succeeds.
- `dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj` succeeds.
- A manual smoke test confirms open, view, rectangle ROI, measure, process, and save.
- README and ROADMAP accurately describe the current scope.
