# Sample Images

This directory contains small generated images for ImageJCsharp manual testing and future regression tests.

All images in this directory are generated specifically for this project and may be redistributed under the same MIT license as the repository.

## Images

| File | Size | Purpose |
|---|---:|---|
| `gradient-8bit.png` | 256 x 128 | Brightness, threshold, histogram, and intensity-range checks |
| `shapes-8bit.png` | 256 x 256 | Rectangle ROI measurement, shape selection, and future particle analysis |
| `edges-8bit.png` | 256 x 256 | Edge detection, Sobel behavior, and threshold boundary checks |

## Manual Smoke Test

Use these steps when checking a new build manually.

1. Open `gradient-8bit.png`.
2. Use zoom in, zoom out, actual size, and fit to window.
3. Run `Process > Threshold...` with a value such as `128`.
4. Save the processed image as a PNG.
5. Open `shapes-8bit.png`.
6. Draw a rectangle ROI around one shape.
7. Run `Analyze > Measure`.
8. Run `Analyze > Export Results...` and save a CSV file.
9. Open `edges-8bit.png`.
10. Run `Process > Find Edges`.

## Notes For Contributors

- Keep sample images small.
- Prefer generated images or images with clear redistribution rights.
- Do not add copyrighted or unclear-license microscope images.
- If adding a sample for a specific bug, document the expected behavior in this file.
