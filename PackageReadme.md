# Fb2 Document Library

Fb2.Document is lightweight, fast .Net 3.1/5/6 lib with bunch of APIs to operate `fb2` file's contents.

Fb2.Document is the easiest way to build reader or editor app for [Fb2](https://en.wikipedia.org/wiki/FictionBook) book format.  

## Table of contents

* [Updates](#updates)

* [Usage examples](#Usage_examples)

## Updates

Latest version brings few changes to make serializing and document tree traversal more convinient.

* `Attributes` where **reworked** - now attributes preserve `NamespaceName` of original `XNode` attribute, using `Fb2Attribute` class. Thus, most of **attributes-related methods** where **updates/reworked**, posing **breaking change**. 

* Added `Parent` property to `Fb2Node` - now each node points back to it's parent, making tree navigation easier.

* Added `NodeMetadata` - preserves `DefaultNamespace` and `Namespace Declarations` attributes of original `XNode` during `Load`, used in serializing bact to `xml` format.

* 

## Usage examples

You can load `Fb2Document` in multiple ways, few are shown below. For more info see [Readme](https://github.com/Overrided/Fb2.Document).

1) From stream

```csharp
Fb2Document fb2Document = new Fb2Document();

using(Stream stream = dataService.GetFileContentStream(Fb2FilePath))
{
    fb2Document.Load(stream);
    // or:
    fb2Document.Load(stream, new Fb2StreamLoadingOptions(false)); // some options
    // or:
    await fb2Document.LoadAsync(stream);
    // or:
    await fb2Document.LoadAsync(stream, new Fb2StreamLoadingOptions(false)); // options
}
```

2) From XDocument

```csharp
XDocument xDocument = dataService.GetFileAsXDoc(Fb2FilePath);

Fb2Document fb2Document = new Fb2Document();

fb2Document.Load(xDocument);
```

3) From string

```csharp
string fileContent = await dataService.GetFileContent(Fb2FilePath);

Fb2Document fb2Document = new Fb2Document();

fb2Document.Load(fileContent);
```
