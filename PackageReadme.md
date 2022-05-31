# Fb2 Document Library&nbsp;[![Fb2.Document CI](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml/badge.svg)](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml)


Fb2.Document is lightweight, fast .Net 3.1/5/6 lib with bunch of APIs to operate `fb2` file's contents.

Fb2.Document is the easiest way to build reader or editor app for [Fb2](https://en.wikipedia.org/wiki/FictionBook) book format.  

Version `2.2.0` brings few improvements to Fb2 document serializing, traversal, attributes and querying.

For more info see [Readme](https://github.com/Overrided/Fb2.Document).

## New features

* Added `Parent` property to `Fb2Node` - now each node points back to it's parent - `Fb2Container` - making tree navigation easier.

* Added `NodeMetadata` property to `Fb2Node` - preserves namespace info of original `XNode` during `Load` method execution.

## Updates

* `Attributes` where **reworked** - now attributes can preserve `NamespaceName` of original `XNode` attribute, using `Fb2Attribute` class. Thus, most of **attributes-related methods** where **updates/reworked**, posing **breaking change**. 

* Updated `Load` and `LoadAsync` methods of `Fb2Document` class - now using `Fb2LoadingOptions` instead of primitives.

* Updated `Fb2Node.Load` method and corresponding overrides to enable `NodeMetadata` usage. 

* Renamed `UnknownNodeException` into `InvalidNodeException`.

* Updated `Fb2Container` methods: `GetChildren(string name)`, `GetFirstChild(string name)`, `GetDescendants(string name)`, `GetFirstDescendant(string name)` - if `name` parameter is not standard Fb2 node name - `InvalidNodeException` is thrown.
