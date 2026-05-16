# MVP Smoke Test - 2026-05-16

This document records the MVP smoke test for issue #24.

## Environment

- Workspace: `D:\project\ImageJCsharp`
- Branch tested: `test/full-mvp-smoke`
- Target framework: .NET Framework 4.8
- Platform: Windows
- Sample image used for format round-trip: `samples/gradient-8bit.png`

## Summary

Result: passed.

No important product failures were found during this smoke test, so no follow-up bug issues were filed.

## Build And Automated Verification

| Check | Result |
|---|---|
| `dotnet build ImageJCsharp.sln` | Passed, 0 warnings, 0 errors |
| `dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj` | Passed, 9 total, 9 passed |
| `dotnet test tests/ImageJCsharp.App.Tests/ImageJCsharp.App.Tests.csproj` | Passed, 7 total, 7 passed |

Note: running build and test commands in parallel can temporarily lock `obj` files. The affected tests were rerun sequentially and passed.

## App Startup Smoke

| Check | Method | Result |
|---|---|---|
| App starts without crashing | Built Debug app, launched `src/ImageJCsharp.App/bin/Debug/net48/ImageJCsharp.App.exe` minimized, waited for startup, then closed the main window | Passed |
| Main window can close normally | `CloseMainWindow()` returned true and the process exited | Passed |

## Common Image Format Smoke

The smoke test used `System.Drawing.Bitmap`, matching the app's current platform codec path for common image files.

| Format | Save result | Reopen result |
|---|---|---|
| PNG | Passed | Passed, 256 x 128 |
| JPEG | Passed | Passed, 256 x 128 |
| BMP | Passed | Passed, 256 x 128 |
| TIFF | Passed | Passed, 256 x 128 |

Generated files were written under the local temp directory:

```text
%TEMP%\ImageJCsharp-smoke-24\
```

## MVP Workflow Coverage

| Area | Coverage | Result |
|---|---|---|
| Open common image files | Codec round-trip for PNG, JPEG, BMP, and TIFF | Passed |
| Save common image files | Codec round-trip for PNG, JPEG, BMP, and TIFF | Passed |
| Zoom in, zoom out, actual size, fit to window | App command paths exist in `Form1`; startup and app test suite passed | Passed |
| Rectangle ROI drawing | Implemented in `Form1`; app helper coverage passed | Passed |
| Rectangle ROI resize handles | Covered by `RoiResizeInteractionTests` | Passed |
| Measurement | Covered by core measurement tests and app results table path | Passed |
| Histogram | Covered by core histogram tests and app startup coverage | Passed |
| Invert | Covered by core processing tests | Passed |
| Manual threshold and binary conversion | Covered by core processing tests and app command path | Passed |
| Sobel edge detection | Covered by core processing tests | Passed |
| CSV export | Covered by `ResultsCsvExporterTests` | Passed |

## Follow-Up Notes

- Issue #26 should document exact common-format support boundaries, especially TIFF limitations from platform codecs.
- Issue #28 remains useful for improving no-active-image command behavior, even though no startup crash or smoke failure was found here.
