# Fb2 Document Library&nbsp;[![Fb2.Document CI](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml/badge.svg)](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml)


Fb2.Document is lightweight, fast .Net 8 lib with bunch of APIs to operate `fb2` file's contents.

Fb2.Document is the easiest way to build reader or editor app for [Fb2](https://en.wikipedia.org/wiki/FictionBook) book format.


# Version 2.3.1

`2.3.1` version of `Fb2.Document` library is basically backport of latest version - `2.5.0` - to .Net 8.

- [Async methods updates](#async-methods-updates)
- [Other Updates](#updates)

For more info please see [Readme](https://github.com/Overrided/Fb2.Document).

## Async Methods Updates

Following methods + corresponding extensions now support cancellation:

* `Fb2Node.AddAttributeAsync`
* `Fb2Element.AddContentAsync`
* `Fb2Container.AddContentAsync`
* `Fb2Container.AddTextContentAsync`
* `Fb2NodeExtensions.AppendAttributeAsync`
* `Fb2ElementExtensions.AppendContentAsync`
* `Fb2ContainerExtensions.AppendContentAsync`
* `Fb2ContainerExtensions.AppendTextContentAsync`

This change can be consedered a **breaking change** since method signatures were changed.

## Other Updates

* `Fb2Node`: introduced `HasAllowedAttributes` property.
* `Fb2LoadingOptions`,`Fb2StreamLoadingOptions`,`Fb2XmlSerializingOptions` are `record`s now.
* `ElementNames` / `AttributeNames` are `struct`s now.