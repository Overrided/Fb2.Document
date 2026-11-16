# Fb2 Document Library&nbsp;[![Fb2.Document CI](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml/badge.svg)](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml)


Fb2.Document is lightweight, fast .Net 10 lib with bunch of APIs to operate `fb2` file's contents.

Fb2.Document is the easiest way to build reader or editor app for [Fb2](https://en.wikipedia.org/wiki/FictionBook) book format.


# Version 2.5.1

Latest release of an `Fb2.Document` library brings performance improvements - both in terms of speed and memory usage.

- [Benchmark Results](#benchmark-results)
- [Other .Net versions](#other-.net-versions)

For more info please see [Readme](https://github.com/Overrided/Fb2.Document).

## Benchmark Results

Benchmarks where run on latest `.Net` versions - `2.5.0` vs `2.5.1` using included test file for all runs.

Despite having backported package to older versions of `.Net` - I didn't re-run benchmarks for all older versions.

TL; DR - ~20-30% speed improvement and ~40% memory usage reduction.

## YOUR MILEAGE MAY AND WILL VARY

### Previous Version (2.5.0):

| Method                                           | Mean      | Error     | StdDev   | StdErr    | Median    | Min      | Q1        | Q3        | Max      | Op/s   | Allocated |
|------------------------------------------------- |----------:|----------:|---------:|----------:|----------:|---------:|----------:|----------:|---------:|-------:|----------:|
| LoadDocument_Full_FromStreamAsync                | 10.816 ms | 0.1166 ms | 3.521 ms | 0.0354 ms |  9.606 ms | 5.810 ms |  8.049 ms | 12.805 ms | 22.77 ms |  92.46 |  16.43 MB |
| LoadDocument_NoUnsafe_FromStreamAsync            | 11.333 ms | 0.0489 ms | 1.427 ms | 0.0149 ms | 11.214 ms | 6.061 ms | 10.910 ms | 12.480 ms | 14.20 ms |  88.24 |  17.24 MB |
| LoadDocument_NoUnsafe_NoMetadata_FromStreamAsync |  8.130 ms | 0.0652 ms | 1.950 ms | 0.0198 ms |  7.263 ms | 6.717 ms |  7.089 ms |  7.589 ms | 13.36 ms | 122.99 |   17.1 MB |


### New Version (2.5.1):

| Method                                           | Mean     | Error     | StdDev   | StdErr    | Median   | Min      | Q1       | Q3       | Max       | Op/s  | Allocated |
|------------------------------------------------- |---------:|----------:|---------:|----------:|---------:|---------:|---------:|---------:|----------:|------:|----------:|
| LoadDocument_Full_FromStreamAsync                | 7.042 ms | 0.0472 ms | 1.361 ms | 0.0143 ms | 6.703 ms | 4.955 ms | 5.705 ms | 8.354 ms | 11.259 ms | 142.0 |  11.07 MB |
| LoadDocument_NoUnsafe_FromStreamAsync            | 7.315 ms | 0.0426 ms | 1.238 ms | 0.0129 ms | 7.983 ms | 4.914 ms | 5.915 ms | 8.261 ms |  9.477 ms | 136.7 |  11.06 MB |
| LoadDocument_NoUnsafe_NoMetadata_FromStreamAsync | 7.524 ms | 0.0365 ms | 1.064 ms | 0.0111 ms | 7.830 ms | 4.719 ms | 7.427 ms | 8.079 ms | 11.646 ms | 132.9 |  10.92 MB |



## Other .Net versions

Version `2.5.1` was back-ported to previous versions of `.Net`, so it still can be used with older projects.

This is the last version to support any version of `.Net` lower than `10.0`.

Please see backported versions table below:

| Package version | .Net version(s) target             |
|-----------------|------------------------------------|
| 1.3.2           | netstandard2.0                     |
| 2.3.2           | netcoreapp3.1;net5.0;net6.0;net7.0 |
| 2.4.2           | net8.0                             |
| 2.5.1           | net10.0                            |