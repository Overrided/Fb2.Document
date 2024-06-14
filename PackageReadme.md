# Fb2 Document Library&nbsp;[![Fb2.Document CI](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml/badge.svg)](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml)


Fb2.Document is lightweight, fast .Net 8 lib with bunch of APIs to operate `fb2` file's contents.

Fb2.Document is the easiest way to build reader or editor app for [Fb2](https://en.wikipedia.org/wiki/FictionBook) book format.


# Version 2.4.0

Latest release of an `Fb2.Document` library brings `.Net` version targeting strategy change, code updates and new minor cosmetic changes.

- [.Net 8](#.net-8)
- [Solution Updates](#code-updates)
- [Code-style Updates](#code-style)

For more info please see [Readme](https://github.com/Overrided/Fb2.Document).

## .Net 8

Starting with version `2.4.0` project will target singular `latest LTS` (Long Term Support) version of `.net`.

Version `2.4.0` introduces this change moving away from multiple framework versions target - `netcoreapp3.1;net5.0;net6.0;net7.0` - and targeting `net8.0` only instead.

This allows to use latest features and reduce package size.

## Solution Updates

* Added `Fb2XmlSerializingOptions`.
* Using `GeneratedRegexAttribute` to improve performance for underlying `RegEx`.
* Exposing `DefaultXmlVersion`, `DefaultXDeclaration` and `DefaultXmlReaderSettings` members of `Fb2Document` class.

## Code-style Updates

* Moved library to `file-scoped namespaces` approach.
* Using `Collection expressions` for `Enumerable` instances initialization.
* Added `.editorconfig`.
