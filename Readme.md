# Fb2 Document Library

Fb2.Document is lightweight, fast .Net Standard lib with bunch of APIs to operate `fb2` file's contents.

Fb2.Document is the easiest way to build reader or editor app for [Fb2](https://en.wikipedia.org/wiki/FictionBook) book format.  

Library can be used with any .Net application that supports .Net Standard 2.0.

## Table of contents

* [Installation](#Installation)

* [Document infrastructure and loading](#Document-infrastructure-and-loading)

* [Usage](#Usage)
    * [Loading file as a string](#Loading-file-as-a-string)
    * [Loading file as XDocument](#Loading-file-as-XDocument)
    * [Loading file as a stream](#Loading-file-as-a-stream)
    * [Loading file as a stream asynchronously](#Loading-file-as-a-stream-asynchronously)
    * [Loading particular node](#Loading-particular-node)
    * [Querying](#Querying)
        * [Content API](#Content-api)
        * [Attributes API](#Attributes-api)
    * [Encoding safety](#Encoding-safety)

* [API](#API)
    * [Fb2Document Class API](#Fb2Document-Class-API)
    * [Fb2Node Class API](#Fb2Node-Class-API)
    * [Fb2Element Class API](#Fb2Element-Class-API)
    * [Fb2Container Class API](#Fb2Container-Class-API)

* [Tests](#Tests)

* [Utils and Constants](#Utils-and-Constants)
    * [Constants](#Constants)
    * [Utils classes](#Utils-classes)

## Installation

You can download and use Fb2Document package in your app via

* NuGet: [Fb2.Document](https://www.nuget.org/packages/Fb2.Document/)

* Package Manager Console: PM> `Install-Package Fb2.Document`

* dotnet cli: `dotnet add package Fb2.Document`

## Document infrastructure and loading

In fact, `fb2` file is just an `XML` with a set of custom nodes, attributes and data relations.

Library creates tree document model similar to what original file had, removing invalid nodes.

Base abstract classes are `Fb2Node` and it's descendants - `Fb2Container` and `Fb2Element`.

`Fb2Node` is lowest abstract level of `fb2` document node implementation.

`Fb2Container` is element, which can - unexpectedly! - contain other elements.

`Fb2Element` is mainly text container element.

`Fb2Document`, in turn, is not derived from `Fb2Node`, but still plays major role in file processing.
That class serves as a container, which is capable of loading and representing `fb2` file structure, providing access to `Fb2Container`/`Fb2Element` basic API's.

`Book` property of an `Fb2Document` is populated after call to `Load` (or `LoadAsync`) method ended and represents actual `<FictionBook>` element of `fb2` file.

>Attention!
>
>During load process all nodes or attributes which do not meet `Fb2` standard will be skipped.
Misplaced elements, like a plain text in root of a book, are not ignored, but marked with `Unsafe` flag instead.
All comments are ignored by default.

For full list of allowed elements and attribute names see [ElementNames](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Constants/ElementNames.cs) and [AttributeNames](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Constants/AttributeNames.cs).

For more details on implementation and supported scenarios see [API](#API) and [Usage](#Usage) sections.

## Usage

As far as file can be read in different ways, `Fb2Document` class provides support for most common scenarios of loading, reading and querying data:

### Loading file as a string

```
string fileContent = await dataService.GetFileContent(Fb2FilePath);

Fb2Document fb2Document = new Fb2Document();

fb2Document.Load(fileContent);
```

>WARNING! Method is not encoding-safe. [*](#Encoding-safety)

### Loading file as XDocument

```
XDocument document = dataService.GetFileAsXDoc(Fb2FilePath);

Fb2Document fb2Document = new Fb2Document();

fb2Document.Load(document);
```

>WARNING! Method is not encoding-safe. [*](#Encoding-safety)

### Loading file as a stream

```
Fb2Document fb2Document = new Fb2Document();

using(Stream stream = dataService.GetFileContentStream(Fb2FilePath))
{
    fb2Document.Load(stream);
}
```

>Method is encoding-safe. [*](#Encoding-safety)

### Loading file as a stream asynchronously

```
Fb2Document fb2Document = new Fb2Document();

using(Stream stream = dataService.GetFileContentStream(Fb2FilePath))
{
    await fb2Document.LoadAsync(stream);
}
```

>Method is encoding-safe. [*](#Encoding-safety)

### Loading particular node

In imaginary scenario you might need to load some part of a document into the model, instead of loading whole thing.

It can be achieved by calling `Load(XNode node)` method of corresponding Fb2Node implementation instance.

```
Paragraph paragraph = new Paragraph();
// assuming node is instance of XNode which contains paragraph - otherwise
// exception will be thrown on validation phase of loading process
paragraph.Load(node);
```

### Querying

All descendants of `Fb2Node` class has certain interface to access and query parsed data.

`Fb2Container`'s descendants provides node content querying interface.

Since `1.1.0` version of a library `Fb2Node` base class provides additional `Attributes` access methods, allowing more complex and precise querying.

#### Content API

For instance, you want to find all `<stanza>` elements in whole book.
There's more than one way to skin a cat:

```
var result = fb2Document.Book.GetDescendants("stanza");
```

or utilizing node name used by library:

```
var result = fb2Document.Book.GetDescendants(ElementNames.Stanza);
```

Also you can query by specifying exact type of descendant node you are looking for:

```
var result = fb2Document.Book.GetDescendants<Stanza>();
```

For full list of properties for accessing document structural parts see [Fb2Document.Properties](#Fb2Document-Properties).

For full list of methods for querying the model's content, see [Fb2Container.Methods](#Fb2Container-Methods).

#### Attributes API

New Api added in version `1.1.0` allows to query node's `Attributes` in different ways.

Let's say you need to check all `id` attributes in a book.

```
var elementsWithIds = fb2Document.Book.GetDescendants<Fb2Node>()
                                 .Where(node => node.HasAttribute(AttributeNames.Id));
```
This will result in all elements with `id` attribute.

```
var allIds = elementsWithIds.Select(node => node.GetAttribute(AttributeNames.Id).Value).ToList();
```
Will get you clean list of all values of `id`'s in `List<string>` form.

Sometimes you can bump into a book, which used cased attribute names, e.g. `Id`, `Name` etc. Examples above will fail due to case-sensitive comparison.

To avoid case-sensitivity issues, pass second parameter, `ignoreCase` (which is optional and `false` by default), as `true`:

```
var elementsIgnoreCaseWithIds = fb2Document.Book.GetDescendants<Fb2Node>()
                                 .Where(node => node.HasAttribute(AttributeNames.Id, true));
```
This will result in all elements with `id`/`Id`/`iD` attribute.

```
var allIgnoreCaseIds = elementsIgnoreCaseWithIds.Select(node => node.GetAttribute(AttributeNames.Id, true).Value).ToList();
```
Will get you clean list of all values of `id`/`Id`/`iD`'s in `List<string>` form.

> Attention!
>
> If node has 2 attributes and only difference in keys is casing e.g. `id`/`Id` - first matching attribute is returned, rest of matches are ignored.

But what if you need to parse value of a particular attribute into `int` or, let's say, `enum`?

Library has `TryGetAttribute` method, which allows next approach:

```
if (cell.TryGetAttributeValue(AttributeNames.Align, true, out string align) &&
    Enum.TryParse<TextAlignment>(align, true, out var textAlignment))
{
    // do some work...
}
```
As one might see, given call ignores case and checks if received value can be used as `TextAlignment`.

For more details on methods for querying the model's attributes, see [Fb2Node.Methods](#Fb2Node-Methods).

### Encoding safety

If method is marked as "not encoding safe" - means content's original encoding is kept, which can cause text symbols rendering issues in case of running into an old encoding like `KOI-8`, cyrillic text, etc.

If method is encoding-safe - during loading process library will try to determine exact encoding of a document and re-encode content of a file info `UTF8`. If automatic encoding detection fails, .Net `Encoding.Default` is used.

## API

## Fb2Document Class API

### Fb2Document Methods

| Method name | Arguments |  Returns  |                              Description                             |
|:-----------:|:---------:|:---------:|----------------------------------------------------------------------|
|     Load    | XDocument |    void   | Loads `Fb2Document` from a `XDocument` synchronously. Not encoding safe! |
|     Load    |   string  |    void   | Loads `Fb2Document` from a string synchronously. Not encoding safe!    |
|     Load    |   Stream  |    void   | Loads `Fb2Document` from a stream synchronously. Encoding safe.        |
|  LoadAsync  |   Stream  |    Task   | Loads `Fb2Document` from a stream asynchronously. Encoding safe.       |
|    ToXml    |           | XDocument | Serializes loaded book back to `XDocument`.                            |
| ToXmlString |           |   string  | Serializes loaded book to `xml` string.                                |
| CreateDocument |        |  Fb2Document | Static method. Introduced in `v1.0.1` of `Fb2.Document` package. Returns new instance of `Fb2Document` with empty `Fb2Book`, `IsLoaded` property is set to `true`.|

### Fb2Document Properties

| Property name |&nbsp;&nbsp;&nbsp;Type&nbsp;&nbsp;&nbsp;|                                                                    Description                                                                    |
|:-------------:|:------------------------:|---------------------------------------------------------------------------------------------------------------------------------------------------|
|      Book     |        FictionBook       | Represents the whole Fb2 file root node - `<FictionBook>`.                                                                                           |
|     Title     |         TitleInfo        | Represents part of `<description>` section, `<title-info>`.                                                                                            |
|  SourceTitle  |       SrcTitleInfo       | Represents part of `<description>` section, `<src-title-info>`. Has same content as Title, but in original language.  Not populated in original book. |
|  DocumentInfo |       DocumentInfo       | Represents part of `<description>` section, `<document-info>`. Contains info about particular fb2 file.                                              |
|  PublishInfo  |        PublishInfo       | Represents part of `<description>` section, `<publish-info>`. Contains info about published (paper) origin of a book.                                |
|   CustomInfo  |        CustomInfo        | Represents part of `<description>` section, `<custom-info>`. Contains any additional info about book.                                                |
|     Bodies    |   IEnumerable\<BookBody>  | Lists all `<body>` elements found in book.                                                                                                          |
|  BinaryImages | IEnumerable\<BinaryImage> | Lists all `<binary>` elements found in book.                                                                                                        |
|    IsLoaded   |           bool           | Indicates whether book was loaded                                                                                                                 |

>Attention!
>
>If `Load()` or `LoadAsync()` methods were not called or had not completed yet, any property of listed above will yield null.

## Fb2Node Class API

`Fb2Node` is base class for all given models in `Fb2Document.Models.*` namespace.

It defines basic interface of a node. Some of descendant classes may override given properties and methods.

### Fb2Node Methods

|   Method name   |                    Arguments                   |            Returns            | Description                                                                                                                                                                                                                                                                                                                                             |
|:---------------:|:----------------------------------------------:|:-----------------------------:|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|       Load      |                      XNode                     |              void             | Performs basic steps - validation of given `XNode` instance and reading attributes of element, if any.                                                                                                                                                                                                                                                  |
|      ToXml      |                                                |            XElement           | Performs basic serialization of node into `fb2`/`xml` format. Creates `XElement` and writes attributes into, if any.                                                                                                                                                                                                                                    |
|   HasAttribute  |                  string,&nbsp;bool                  |              bool             | Introduced in `v1.1.0`. <br>Checks if node has attribute(s) with given key. <br>`bool ignoreCase` param is optional, set to `false` by default. Set it to `true` to ignore casing during key search. <br> If `key` argument is null or empty string, `ArgumentNullException` is thrown.                                                                        |
|   GetAttribute  |                  string,&nbsp;bool                  | KeyValuePair\<string,&nbsp;string> | Introduced in `v1.1.0`. <br>Returns the first element of `Attributes` list that matches given key or a `default` value if no such element is found. <br> `bool ignoreCase` param is optional, set to `false` by default. Set it to `true` to ignore casing during key search.<br>If `key` argument is null or empty string, `ArgumentNullException` is thrown. |
| TryGetAttribute | string,&nbsp;bool, out&nbsp;KeyValuePair<string,&nbsp;string> |              bool             | Introduced in `v1.1.0`. <br>Returns true if attribute found by given key, otherwise `false`. <br>If none attribute found, result contains `default` value of `KeyValuePair<string, string>` instead of actual attribute.<br>If `key` argument is null or empty string, `ArgumentNullException` is thrown.                                                     |

### Fb2Node Properties

|   Property name   |&nbsp;&nbsp;&nbsp;Type&nbsp;&nbsp;&nbsp;|                                                                                                Description                                                                                                |
|:-----------------:|:--------------------------:|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|        Name       |           string           | Name of element. Abstract property, each element must implement it.                                                                                                                                       |
|     Attributes    | Dictionary\<string,&nbsp;string> | Actual attributes of `XNode` are mapped into this property (if any).                                                                                                                                        |
| AllowedAttributes |       HashSet\<string>      | List of names of allowed attributes. Any attribute,  not belonging to element, will be skipped.                                                                                                           |
|      IsInline     |            bool            | Indicates whether element should go in same line or after new line.                                                                                                                                       |
|       Unsafe      |            bool            | Indicates if element is placed correctly in document structure. For instance, `<p>` element is not expected directly in `<FictionBook>` element. So paragraph will not be skipped, but marked as Unsafe. |

## Fb2Element Class API

Derived from `Fb2Node`. `Fb2Element` is basic abstract class for any text-only container element.

`IsInline` property is set to `true` by default for any descendant of `Fb2Element`.

### Fb2Element Methods

| Method name | Arguments |  Returns |                                     Description                                      |
|:-----------:|:---------:|:--------:|--------------------------------------------------------------------------------------|
|     Load    |   XNode   |   void   | Override. Copies given `XNode`s string content, providing validation & formatting.    |
|    ToXml    |           | XElement | Override. Populates element with string content.                                     |
|   ToString  |           |  string  | Override. Returns element's `Content` property.                                        |

### Fb2Element Properties

| Property name |  Type  |                                        Description                                        |
|:-------------:|:------:|-------------------------------------------------------------------------------------------|
|    Content    | string | Content (value) of mapped `XElement` element. Available after `Load()` method call completes. |
|    IsInline   | bool   | Override of base `IsInline` property from `Fb2Node` class. `true` by default.     |

## Fb2Container Class API

Derived from `Fb2Node`. `Fb2Container` is basic abstract class for any element, capable of containing elements.

### Fb2Container Methods

Base (inherited) methods

| Method name |&nbsp;&nbsp;&nbsp;Arguments&nbsp;&nbsp;&nbsp;|  Returns | Description                                                                                                                  |
|:-----------:|:----------:|:--------:|------------------------------------------------------------------------------------------------------------------------------|
|     Load    | XNode |   void   | Override. Copies given `XNode`s content, and calls `Load` method recursively on each child element, propagating tree loading. |
|   ToString  |       |  string  | Override. Calls `ToString` method recursively on each child element and  combines it based on `IsInline` property.           |
|    ToXml    |       | XElement | Override. Populates `XElement` with content recursively, by calling `ToXml` on each child.                                     |

Node querying methods

|       Method name      |Arguments|        Returns        | Description                                                                                                                                                                                                                                                             |
|:----------------------:|:-----------:|:---------------------:|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|       GetChildren      | string | IEnumerable\<Fb2Node> | Gets children of element by given name.  Wrapper for `Content.Where(e => e.Name.EqualsInvariant(name))` query. `name` parameter is case insensitive.  If `name` is null or empty string, `ArgumentNullException` is thrown.                                                                             |
|      GetFirstChild     | string |        Fb2Node        | Gets first child of element by given name.  Wrapper for `string.IsNullOrWhiteSpace(name) ? Content.FirstOrDefault() : Content.FirstOrDefault(e => e.Name.EqualsInvariant(name));` query. `name` parameter is case insensitive.                                                                          |
|     GetDescendants     | string |     List\<Fb2Node>    | Gets all descendants of an element by given name. Recursively iterates through sub-tree, filtering elements by name. `name` parameter is case insensitive.  If `name` is null or empty string, *all* descendant elements are  returned in flat list format.                                             |
|   GetFirstDescendant   | string |        Fb2Node        | Gets first descendant of an element by given name. Recursively iterates through sub-tree, filtering elements by name. `name` parameter is case insensitive. If `name` is null or empty string, `ArgumentNullException` is thrown.                                                                       |
|     GetChildren\<T>    |        |    IEnumerable\<T>    | Gets children of element by given type `T`, `where T : Fb2Node`. For example, `fb2Document.Book.GetChildren<BookDescription>()`  query returns all `BookDescription` elements from `fb2Document.Book.Content`                                                                                           |
|    GetFirstChild\<T>   |        |           T           | Gets first child of element by given type `T`, `where T : Fb2Node`. For example, `fb2Document.Book.GetFirstChild<BookDescription>()`  query returns first `BookDescription` element from `fb2Document.Book.Content`                                                                                     |
|   GetDescendants\<T>   |        |        List\<T>       | Gets all descendants of an element by given type `T`, `where T : Fb2Node`. Recursively iterates through sub-tree, filtering elements by type. For example, `fb2Document.Book.GetDescendants<Paragraph>()`  query returns all `Paragraph` element from whole book (as fb2Document.Book is root element). |
| GetFirstDescendant\<T> |        |           T           | Gets first descendant of an element by given type `T`, `where T : Fb2Node`. Recursively iterates through sub-tree, filtering element by type. For example, `fb2Document.Book.GetFirstDescendant<Paragraph>()`  query returns first `Paragraph` element it meets in whole book.                          |

At some point you may want to list whole node`s content in list (flat) form.

To achieve this, use `GetDescendants` method(s).

For `GetDescendants(string name)` implementation:

```
var wholeBookContent = fb2Document.Book.GetDescendants(null);
```

For `GetDescendants<T>()` implementation:

```
var wholeBookContent = fb2Document.Book.GetDescendants<Fb2Node>();
```

## Tests

Solution contains test project, which covers all models configurations & main element loading, validation and generation logic.

As far as project contains 63 different models, which are, in fact, configs for loading of different nodes from files,
it would be incredibly boring to write tests for each of it. Reflection comes as best solution to get all model types, create corresponding instances and try to load
each of them with different sets of data.

Also, there are separate tests for [Fb2ElementFactory](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Factories/Fb2ElementFactory.cs) and  
`IntegrationTests` test, which creates `Fb2Document`, saves it as a file, reads it into second model, saves again and checks if both saved files are equal.

## Utils and Constants

Also, library provides bunch of constants and utils to help end-user to deal with `fb2` file format.

### Constants

Most used constants are [ElementNames](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Constants/ElementNames.cs) and [AttributeNames](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Constants/AttributeNames.cs) which contain,
respectively, element and attribute names that are allowed to be loaded from document.

All nodes / attributes, which names are not on list, will be omitted during document loading.

### Utils classes

Library has some helper classes to work with:

* Stream extension: `SeekZero,CloneAsync,Clone`

* String extension: `EqualsInvariant`

* XNode extension: `GetAllAttributesOrDefault,Validate,GetNodeContent`
