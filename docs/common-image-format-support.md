# Common Image Format Support

This note records common image file open/save behavior for issue #26.

## Environment

- Date: 2026-05-16
- Workspace: `D:\project\ImageJCsharp`
- Verification sample: `samples/gradient-8bit.png`
- Codec path: `System.Drawing.Bitmap`, matching the current app implementation

## Summary

PNG, JPEG, BMP, and basic TIFF round trips passed with the current platform codecs.

The current MVP image pipeline converts opened images into the internal grayscale `GrayImage` model for processing. This means common file formats can be opened and saved, but advanced format metadata and scientific image semantics are not preserved.

## Round-Trip Result

The verification saved `samples/gradient-8bit.png` to each target format, then reopened the saved file with `System.Drawing.Bitmap`.

| Format | Save result | Reopen result | Reopened pixel format |
|---|---|---|---|
| PNG | Passed | Passed, 256 x 128 | `Format32bppArgb` |
| JPEG | Passed | Passed, 256 x 128 | `Format24bppRgb` |
| BMP | Passed | Passed, 256 x 128 | `Format32bppRgb` |
| TIFF | Passed | Passed, 256 x 128 | `Format32bppArgb` |

Generated files were written to:

```text
%TEMP%\ImageJCsharp-format-26\
```

## Support Boundaries

Current MVP support:

- Open PNG, JPEG, BMP, and basic TIFF through platform image codecs.
- Save PNG, JPEG, BMP, and TIFF through platform image codecs.
- Convert opened images to grayscale for current processing commands.
- Preserve image dimensions during the verified round trip.

Known limitations:

- TIFF support means basic platform-codec TIFF only.
- OME-TIFF, multi-page TIFF stacks, scientific TIFF metadata, and vendor microscopy metadata are not part of the current MVP support.
- JPEG is lossy by format design, so exact pixel values should not be assumed after saving.
- RGB, 16-bit, 32-bit float, multi-channel, stack, and hyperstack preservation are deferred.

No important format failures were found during this verification, so no follow-up bug issue was filed.
