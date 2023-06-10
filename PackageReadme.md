# Fb2 Document Library&nbsp;[![Fb2.Document CI](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml/badge.svg)](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml)


Fb2.Document is lightweight, fast .Net 3.1/5/6/7 lib with bunch of APIs to operate `fb2` file's contents.

Fb2.Document is the easiest way to build reader or editor app for [Fb2](https://en.wikipedia.org/wiki/FictionBook) book format.  

Version `2.3.0` brings `net7.0` support alongside with bugfixes, new `APIs` and improved test coverage.

For more info see [Readme](https://github.com/Overrided/Fb2.Document).

## Solution changes

* Added `net7.0` support.
* Added `Fb2.Document.Benchmark`
* Added `Fb2.Document.Playground`
* Refactored `Fb2.Document.Tests`

## New features

* `Fb2Node`:
    * Added `HasAttributes` property

* `Fb2Element`:
    * Added `AddContentAsync` method

* `Fb2Container`:
    * Added `AddTextContent` method
    * Added `AddTextContentAsync` method

## Updates

* `Fb2Node`:
    * Renamed `IsEmpty` Property to `HasContent` - breaking change
    * Fixed `Attributes` loading bug
* `SequenceInfo`, `Image`, `TextLink`:
    * Updated `ToString` override
* `Lang`, `SrcLang`:
    * `IsInline` property returns false now
* `EmptyLine`:
    * removed `HasContent` (`IsEmpty`) override. Now, `EmptyLine.HasContent` returns `true`. It's somewhat counter-intuitive, but well, technically `Environment.NewLine` that's `content` of `EmptyLine` is not `null` or `empty` string :)
* Renamed `UnexpectedAtrributeException` to `UnexpectedAttributeException`