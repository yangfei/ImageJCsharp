# Release Process

This document describes how ImageJCsharp should publish releases.

The project should release early and regularly. Releases do not need to be perfect, but each release should be clear about what works and what is known to be incomplete.

## Versioning

Use semantic-style versions:

- `v0.1.0` for the first public development release.
- `v0.2.0`, `v0.3.0`, etc. for new MVP capabilities.
- `v1.0.0` when the project reaches the first stable ImageJ-like core replacement target.

Before `v1.0.0`, APIs and behavior may change.

## Suggested Early Releases

### v0.1.0

Goal:

- Public GitHub repository.
- Build and test automation.
- First downloadable Windows build.
- Basic app startup.

### v0.2.0

Goal:

- Open common image formats.
- Display image.
- Zoom and scroll.
- Save image.

### v0.3.0

Goal:

- Rectangle ROI.
- Measure ROI.
- Results table.

### v0.4.0

Goal:

- Invert.
- Threshold.
- Sobel edge detection.

### v0.5.0

Goal:

- Histogram.
- Export results.
- Better manual test images.

## Pre-Release Checklist

Before creating a release:

- [ ] Update `CHANGELOG.md`.
- [ ] Confirm `README.md` describes current status accurately.
- [ ] Run `dotnet build ImageJCsharp.sln`.
- [ ] Run `dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj`.
- [ ] Manually start the app.
- [ ] Manually open a PNG.
- [ ] Manually draw a rectangle ROI.
- [ ] Manually measure.
- [ ] Manually save an image.
- [ ] Note known limitations.

## Building A Release Zip

Build release binaries:

```powershell
dotnet build src/ImageJCsharp.App/ImageJCsharp.App.csproj -c Release
```

Create a zip from:

```text
src/ImageJCsharp.App/bin/Release/net48/
```

Suggested zip name:

```text
ImageJCsharp-v0.1.0-win-x64.zip
```

## Creating A GitHub Release

1. Go to the GitHub repository.
2. Open the Releases page.
3. Click "Draft a new release".
4. Create a new tag, for example `v0.1.0`.
5. Use the release title `ImageJCsharp v0.1.0`.
6. Upload the zip file.
7. Paste release notes.
8. Mark as pre-release while the project is before `v1.0.0`.
9. Publish.

## Release Notes Template

```markdown
## ImageJCsharp v0.1.0

This is an early development release of ImageJCsharp, a C# native ImageJ-like application for Windows.

### Added

- 

### Changed

- 

### Fixed

- 

### Known limitations

- 

### Verification

- `dotnet build ImageJCsharp.sln`
- `dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj`
```

## After Release

- Share release link on project channels.
- Add screenshots or GIFs.
- Open issues for known limitations.
- Ask users for small sample images and workflow feedback.
