# Fb2 Document Library&nbsp;[![Fb2.Document CI](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml/badge.svg)](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml)


Fb2.Document is lightweight, fast .Net 10 lib with bunch of APIs to operate `fb2` file's contents.

Fb2.Document is the easiest way to build reader or editor app for [Fb2](https://en.wikipedia.org/wiki/FictionBook) book format.


# Version 2.5.0

Latest release of an `Fb2.Document` library brings `.Net 10` and few minor updates.

- [.Net 10](#.net-10)
- [Other Updates](#updates)

For more info please see [Readme](https://github.com/Overrided/Fb2.Document).

## .Net 10

With version `2.5.0` project targets `.Net 10`.

## Other Updates

* `Fb2LoadingOptions`,`Fb2StreamLoadingOptions`,`Fb2XmlSerializingOptions` are `record`s now.
* `ElementNames` / `AttributeNames` are `struct`s now.
* `Fb2Node`: introduced `HasAllowedAttributes` property.
* Following methods + corresponding extensions now support cancellation:
	* `Fb2Node.AddAttributeAsync`
	* `Fb2Container.AddContentAsync`
	* `Fb2Element.AddContentAsync`
	* `Fb2NodeExtensions.AppendAttributeAsync`
	* `Fb2ContainerExtensions.AppendContentAsync`
	* `Fb2ElementExtensions.AppendContentAsync`
