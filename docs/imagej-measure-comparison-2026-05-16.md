# ImageJ Measure Comparison - 2026-05-16

This document records the first focused comparison between ImageJ Measure and
ImageJCsharp Measure behavior.

## Environment

| Item | Value |
| --- | --- |
| ImageJ build | ImageJ 1.54s99 |
| ImageJ source | `https://wsr.imagej.net/jars/ij.jar` |
| Java | `C:\Java21\jdk-21.0.2\bin\java.exe` |
| ImageJCsharp commit compared initially | `762eae3` |
| Operating system | Windows |

ImageJ was run through its Java API with `ij.plugin.filter.Analyzer` using these
measurement flags:

```text
AREA | MEAN | MIN_MAX | STD_DEV
```

ImageJCsharp was run through `ImageJCsharp.Core.Measurements.Measure` with
`PixelCalibration.Identity`.

## Test Image

The comparison used a synthetic 4 x 3 8-bit grayscale image.

```text
0    10   20   30
40   50   60   70
80   90   100  110
```

The rectangle ROI used ImageJ/ImageJCsharp zero-based coordinates:

```text
x = 1
y = 1
width = 2
height = 2
```

The ROI covers these four pixels:

```text
50   60
90   100
```

## Results

### Full Image

| Measurement | ImageJ 1.54s99 | ImageJCsharp at `762eae3` | Initial Status |
| --- | ---: | ---: | --- |
| Area | 12 | 12 | Match |
| Mean | 55 | 55 | Match |
| Min | 0 | 0 | Match |
| Max | 110 | 110 | Match |
| StdDev | 36.05551275463989 | 34.5205252953466 | Differs |

### Rectangle ROI

| Measurement | ImageJ 1.54s99 | ImageJCsharp at `762eae3` | Initial Status |
| --- | ---: | ---: | --- |
| Area | 4 | 4 | Match |
| Mean | 75 | 75 | Match |
| Min | 50 | 50 | Match |
| Max | 100 | 100 | Match |
| StdDev | 23.804761428476166 | 20.6155281280883 | Differs |

## Compatibility Notes

The first comparison shows that area, mean, minimum, and maximum match for the
tested full-image and rectangle-ROI cases.

Standard deviation differs. ImageJ reports the sample standard deviation for
these cases, while ImageJCsharp currently reports the population standard
deviation.

## Follow-Up

Follow-up issue created from this comparison:

```text
https://github.com/yangfei/ImageJCsharp/issues/57
```

Issue #57 aligned ImageJCsharp Measure with ImageJ for these StdDev cases by
using sample standard deviation in `Measurements.Measure`. Automated Core tests
now assert the ImageJ 1.54s99 full-image and rectangle-ROI StdDev values listed
above.
