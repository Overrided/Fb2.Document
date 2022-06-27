# Fb2 Document Library&nbsp;[![Fb2.Document CI](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml/badge.svg)](https://github.com/Overrided/Fb2.Document/actions/workflows/ci_build.yml)

Fb2.Document is lightweight, fast .Net Standard 2.0 lib with bunch of APIs to operate `fb2` file's contents.

Fb2.Document is the easiest way to build reader or editor app for [Fb2](https://en.wikipedia.org/wiki/FictionBook) book format.  

## Table of contents

* [Installation](#Installation)

* [Document infrastructure](#Document-infrastructure)

* [Loading](#Loading)
    * [Fb2Document](#fb2document)
    * [...particular node](#particular-node)

* [Encoding](#encoding)

* [Querying](#querying)
    * [Query Fb2Element](#query-fb2element-content)
    * [Query Fb2Container](#query-fb2Container)
    * [Querying Attributes](#querying-attributes)

* [Editing](#Editing)
    * [Editing Fb2Element content](#editing-fb2element-content)
    * [Editing Fb2Container content](#editing-fb2container-content)
    * [Editing Attributes](#editing-attributes)
    * [Method chaining](#method-chaining)

* [Extensions](#extensions)

* [Constants](#Constants)

* [Error handling](#Error-handling)

* [Tests](#Tests)


## Installation

You can download and use Fb2Document package in your app via

* NuGet: [Fb2.Document](https://www.nuget.org/packages/Fb2.Document/)

* Package Manager Console: PM> `Install-Package Fb2.Document`

* dotnet cli: `dotnet add package Fb2.Document`


## Document infrastructure

In fact, `fb2` file is just an `XML` with a set of custom nodes, attributes and data relations. However, validation of given `fb2` file against appropriate `xsd` scheme is not an option as there are LOTS of semi-valid `fb2` files in the wild.

So, `xsd` scheme validation is ommited.

Library creates tree document model similar to what original file had, removing invalid nodes.

Base abstract classes are `Fb2Node` and it's descendants - `Fb2Container` and `Fb2Element`.

`Fb2Node` is lowest abstract level of `fb2` document node implementation - it implements all `Attributes`-related APIs.

`Fb2Container` is container node, which can - unexpectedly! - contain other elements (nodes).

`Fb2Element` is mainly text container element.

`Fb2Document` class, in turn, is not derived from `Fb2Node`, but still plays major role in file processing.
That class serves as a topmost container, which is capable of loading and representing `fb2` file structure, providing access to `Fb2Container`/`Fb2Element` basic API's.

`Book` property of an `Fb2Document` is populated after call to `Load` (or `LoadAsync`) method ended and represents actual `<FictionBook>` element of `fb2` file - root of a `Fb2 DOM` tree.

>Attention!
>
>During load process all nodes or attributes which do not meet `Fb2` standard will be skipped.
Misplaced elements, like a plain text in root of a book, are not ignored, but marked with `IsUnsafe` flag instead.
All comments are ignored by default.

For full list of allowed elements and attribute names see [ElementNames](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Constants/ElementNames.cs) and [AttributeNames](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Constants/AttributeNames.cs).


## Loading

There are API's that enable loading `Fb2 DOM` in different scenarios, for both `Fb2Document` and any particular node.

### Fb2Document

All `Load` or `LoadAsync` methods of `Fb2Document` class accept either `Fb2LoadingOptions` or `Fb2StreamLoadingOptions` optional parameter to configure content loading. For more info see [LoadingOptions](https://github.com/Overrided/Fb2.Document/tree/master/Fb2.Document/LoadingOptions).

Examples of loading `Fb2Document`...

1) ...from string

```csharp
string fileContent = await dataService.GetFileContent(Fb2FilePath);

Fb2Document fb2Document = new Fb2Document();

fb2Document.Load(fileContent);
```

>WARNING! Method is not encoding-safe. [*](#Encoding-safety)

2) ...from XDocument

```csharp
XDocument xDocument = dataService.GetFileAsXDoc(Fb2FilePath);

Fb2Document fb2Document = new Fb2Document();

fb2Document.Load(xDocument);
```

>WARNING! Method is not encoding-safe. [*](#Encoding-safety)

3) ...from stream

```csharp
Fb2Document fb2Document = new Fb2Document();

using(Stream stream = dataService.GetFileContentStream(Fb2FilePath))
{
    fb2Document.Load(stream);
    // or:
    fb2Document.Load(stream, new Fb2StreamLoadingOptions(false)); // some options
}
```

>Method is encoding-safe. [*](#Encoding-safety)

4) ...from stream asynchronously

```csharp
Fb2Document fb2Document = new Fb2Document();

using(Stream stream = dataService.GetFileContentStream(Fb2FilePath))
{
    await fb2Document.LoadAsync(stream);
    // or:
    await fb2Document.LoadAsync(stream, new Fb2StreamLoadingOptions(false)); // options
}
```

>Method is encoding-safe. [*](#Encoding-safety)

### ...particular node

In corner-case scenario you might need to load some part of a document into the model, instead of loading whole thing.

It can be achieved by directly calling `Load(XNode node)` method on corresponding Fb2Node implementation instance.

```csharp
Paragraph paragraph = new Paragraph();
// assuming node is instance of XNode which contains paragraph - otherwise
// exception will be thrown on validation phase of loading process
paragraph.Load(node);
```


## Encoding

If method is marked as "not encoding safe" - means content's original encoding is kept, which can cause text symbols rendering issues in case of running into an old encoding like `KOI-8`, cyrillic text, etc.

If method is encoding-safe - during loading process library will try to determine exact encoding of a document and re-encode content of a file info `UTF8`. If automatic encoding detection fails, .Net `Encoding.Default` is used.


## Querying

All descendants of `Fb2Node` (itself included) class provide build-in methods to query and manipulate parsed data in multiple ways.


### Query Fb2Element

`Fb2Element` class desendants APIs allows to work with plain text content, `leaves` of the `fb2 DOM` tree. 

`Content` property of `Fb2Element` is get-only to prevent random and/or incorrect changes.

So you read it exactly how you would read any other string, no specific string-search API is provided.

Check if particular `fb2Element` has substring:

```csharp
var hasSubstring = fb2Element.Content.Contains(...);
```


### Query Fb2Container

`Fb2Container` class descendants provides node content managing APIs.

Some queries like `fb2Container.Content.Where(n => n is Fb2Paragraph)` or `fb2Node.Attributes.ContainsKey(...)` are pre-baked into library to simplify data access.

Code below demonstrates few simple scenarios on how to select nodes directly from `Content` property.

1) Finding all `Image` nodes in given `Paragraph` container node:

```csharp
var allImagesInParagraphByName = paragraph.GetChildren(ElementNames.Image);
var allImagesInParagraphByPredicate = paragraph.GetChildren(n => n.Name == ElementNames.Image);
var allImagesInParagraphByType = paragraph.GetChildren<Image>();
```

2) Looking for ***first*** `Image` node in given `Paragraph` container node:

```csharp
var firstImageInParagraphByName = paragraph.GetFirstChild(ElementNames.Image);
var firstImageInParagraphByPredicate = paragraph.GetFirstChild(n => n.Name == ElementNames.Image);
var firstImageInParagraphByType = paragraph.GetFirstChild<Image>();
```


### Query Fb2Container sub-tree

`Descendant` node is contained either in `Content` propery of given element directly, or is contained further in `Fb2 DOM` sub-tree (indirect content).

`Fb2Container` built-in APIs allow to search for descendants nodes:

1) Finding all `Image` nodes in given `BookSection` container node:

```csharp
IEnumerable<Fb2Node> allSectionImagesByName = bookSection.GetDescendants(ElementNames.Image); // by string Name
IEnumerable<Fb2Node> allSectionImagesByPredicate = bookSection.GetDescendants(n => n.Name == ElementNames.Image); // with Func<Fb2Node, bool> predicate
IEnumerable<Image> allSectionImagesByType = bookSection.GetDescendants<Image>(); // with type parameter, where T : Fb2Node
```

2) Finding ***first*** `Subscript` node in `BookBody` (including first-level content):

```csharp
Fb2Node? firstSubscriptInBookBodyByName = bookBody.GetFirstDescendant(ElementNames.Subscript);
Fb2Node? firstSubscriptInBookBodyByPredicate = bookBody.GetFirstDescendant(node => node is Subscript);
Subscript? firstSubscriptInBookBodyByType = bookBody.GetFirstDescendant<Subscript>();
```

3) Attempt to both check if any matching `node` exists and get ***first*** matching `Fb2Node instance` at the same time:

```csharp
bool hasCustomInfoByName = bookBody.TryGetFirstDescendant(ElementNames.CustomInfo, out Fb2Node? firstCustomInfoByName);
if(hasCustomInfoByName)
    Console.WriteLine(firstCustomInfoByName.ToString());

bool hasCustomInfoByPredicate = bookBody.TryGetFirstDescendant(n => n.Name == ElementNames.CustomInfo, out Fb2Node? firstCustomInfoByPredicate);
if(hasCustomInfoByPredicate)
    Console.WriteLine(firstCustomInfoByPredicate.ToString());

bool hasCustomInfoByType = bookBody.TryGetFirstDescendant<CustomInfo>(out CustomInfo? firstCustomInfoByType);
if(hasCustomInfoByType)
    Console.WriteLine(firstCustomInfoByType.ToString());
```

> Attention!
>
> Methods `GetChildren` and `GetDescendants` never returns null. Instead, empty `IEnumerable` is always returned. 


### Querying Attributes

`Fb2Node` base class provides `Attributes` access and modifications methods.

Lots of operations with `Fb2Document` - like searching / rendering / querying are heavily dependant on `Attributes`.

So there are few methods designed to simplify `Attributes` reading.

For more details on methods for querying node attributes, see [Fb2Node.Methods](#Fb2Node-Methods).

1) Check if `Fb2Node` has `Attribute` with patricular `Name` (in this case `Id`):

```csharp
bool hasAttributeByKey = fb2Node.HasAttribute(AttributeNames.Id);
bool hasAttributeByKeyCaseIgnore = fb2Node.HasAttribute("ID", true); // second parameter indicates case-insensitive comparison
```

2) Get `Attribute` of a node in `Fb2Attribute` form by `Name`:

```csharp
Fb2Attribute? attribute = fb2Node.GetAttribute(AttributeNames.Name);
Fb2Attribute? attributeCaseIgnore = fb2Node.GetAttribute(AttributeNames.Name, true);
```

> Attention!
>
> `GetAttribute(string key, bool ignoreCase = false)` method returns `null` if there's no attribute found by given name.

3) Checking if attribute is there while retrieving it's value in `Fb2Attribute` form at a same time:

```csharp
bool hasAttribute = fb2Node.TryGetAttribute(AttributeNames.Name, out Fb2Attribute attributeResult);
bool hasAttributeCaseIgnore = fb2Node.TryGetAttribute(AttributeNames.Name, true, out Fb2Attribute attributeResultCaseIgnore);
``` 

4) Need to parse attribute value into `Enum` or something?

```csharp
if (tableCellFb2Node.TryGetAttribute(AttributeNames.Align, true, out var alignAttribute) &&
    Enum.TryParse<TextAlignment>(alignAttribute.Value, true, out var textAlignment))
{
    //...do work
}
```


## Editing

All descendants of `Fb2Node` class provide content manipulation APIs along with `Attributes` modification methods.
All content of `Fb2Document` is represented by instances of two core classes:

 - `Fb2Element` - represents `plain text` node of some kind.
 - `Fb2Container` - represents node capable of containing other nodes along with text.

Naturally, both types provide different APIs for editing respective `Content` property - `string` for `Fb2Element` and `ImmutableList<Fb2Node>` for `Fb2Container`.


### Editing `Fb2Element` content

As far as `Fb2Element` is purely-text representation entity, working with it looks like working with a `string`.  
While this is true for `reading` string `Content`, this is not exactly the case for `changing` it as whole `fb2` format relies on `xml`. All content - nodes, text, `Attributes` - should comply with `xml` format, which brings some restrictions.

1) Adding new text content in simplest manner is using `TextItem`, which directly represents plain text.

```csharp
var textItem = new TextItem().AddContent("Hello,"); // no separator parameter
```

2) Consider `textItem` from example above. It's missing something...

```csharp
textItem.AddContent(() => "World!", " "); // second parameter is separator, used during appending new content to existing one.

var updatedContent = textItem.Content;
Debug.WriteLine(updatedContent);
// produces:
// "Hello, World!"
```

> Attention!
>
> Due to metioned `xml` format limitations, both parameters - `newContent` and `separator` in `AddContent` method are escaped by replacing `Environment.NewLine` with " " (whitespace) and symbols `<`, `>`, `&`, `'`, `"` with encoded counterparts - `&lt;`, `&gt;`, `&quot;` etc. 


3) To clear `Content`:

```
fb2Element.ClearContent();
```


### Editing `Fb2Container` content

If given `Fb2Container` node can contain `text` nodes (indicated by `CanContainText` property) along with other nodes. Text can be inserted via special method:

1) Adding 'hello world' `text` to `Strikethrough` container:

```csharp
var strikethrough = new Strikethrough();
strikethrough.AddTextContent("hello world"); // adding text directly
```

For any descendant of `Fb2Container` basic method of adding nodes to actual content is `AddContent(Fb2Node node)`, with a bunch of overloads on top for different use-cases.

2) Adding `empty Strong` node to `Paragraph`:

```csharp
var paragraph = new Paragraph();
paragraph.AddContent(new Strong()); // adding empty child node
```

3) Adding `Strong` node `with text` to `Paragraph` :

```csharp
var paragraph = new Paragraph();
paragraph.AddContent(new Strong().AddTextContent("strong content")); // adding child instance with text
```

4) Adding `empty Strong` node to `Paragraph` using `LINQ`-style method chaining:

```csharp
var paragraph = new Paragraph();
paragraph.AddContent(ElementNames.Strong) // by node name
         .AddContent(new List<Fb2Node> { new Strong(), new Emphasis() }) // add multiple items at once as `IEnumerable<Fb2Node>`
         .AddContent(new Strong(), new Emphasis()) // add multiple items at once as `params Fb2Node[]`
         .AddContent(() =>                          // with node function provider - Func<Fb2Node>
         {
             // do stuff, load, query content, etc
             DoWork();
             return new Strong().AddTextContent("sync bold text provider");
         });
await paragraph.AddContentAsync(async () =>          // with async node function provider - Func<Task<Fb2Node>>
{
    // do async stuff, load, query content, etc.
    var strongText = await GetStrongTextValue();
    return new Strong().AddTextContent(strongText);
});
```

5) To remove particular node / set of nodes, use `RemoveContent` method or one of it overloads:

```csharp
// set up

var strong = new Strong().AddTextContent("strong content ");
var italic = new Emphasis().AddTextContent("emphasis content ");
var strikethrough = new Strikethrough().AddTextContent("strikethrough content ");

var paragraph = new Paragraph().AddContent(strong, italic, strikethrough).AddTextContent("plain text ");

// remove content how you see fit
paragraph.RemoveContent(new List<Fb2Node> { strong, italic }); // drop strong and italic nodes using IEnumerable<Fb2Nodes>
paragraph.RemoveContent(n => n is Strong || n is Emphasis); // drop strong and italic nodes using Func<Fb2Node, bool> predicate 
paragraph.RemoveContent(strong); // drop particular node
```

6) To clean all content of `Fb2Container` (`paragraph` from example above):

```csharp
paragraph.ClearContent();
```


### Editing Attributes

`Fb2Node` base class provides `Attributes` access and modifications methods.

1) To add single attribute to given `Fb2Node`, use overloaded `AddAttribute` method:

```csharp
var paragraph = new Paragraph();

paragraph.AddAttribute(AttributeNames.Id, "paragraph_id"); // adding single attribute by key & value
// or:
paragraph.AddAttribute(new Fb2Attribute("id", "paragraph_id")); // adding single attribute
// or:
paragraph.AddAttribute(() => new Fb2Attribute("id", "paragraph_id")); // adding single attribute via provider function
// or:
await paragraph.AddAttributeAsync(async () => { // adding single attribute via async provider function
    var kvp = await attributeService.GetAttributeAsync();
    return kvp;
});
```

2) To add multiple attributes to given `Fb2Node` at once, use overloaded `AddAttributes` method:

```csharp
var paragraph = new Paragraph();

paragraph.AddAttributes(
        new Fb2Attribute(AttributeNames.Id, "testId"), 
        new Fb2Attribute(AttributeNames.Language, "eng")); // params Fb2Attribute[] attributes
// or:
paragraph.AddAttributes(new List<Fb2Attribute>{ new Fb2Attribute(AttributeNames.Id, "testId"), new Fb2Attribute(AttributeNames.Language, "eng") });
```

3) To remove particular attribute / set of attributes, use overloaded `RemoveAttribute` method:

```csharp

// remove attributes
string attributeName = ....;
paragraph.RemoveAttribute(attributeName); // removing attribute by Key, case sensitive! 
// or:
paragraph.RemoveAttribute(attributeName, true); // removing attribute by Key, case insensitive!
// or:
paragraph.RemoveAttribute(attr => attr.Key.Equals(attributeName)); // removing attribute by predicate

```

4) To clear `Attributes` of a given node, use `ClearAttributes`:

```csharp
paragraph.ClearAttributes();
```


### Method chaining

Editing API calls can be chained, because each editing API call return entity which is being edited itself, allowing calls like this: 

```csharp
var paragraph = new Paragraph();
paragraph
    .AddContent(new Strong().AddTextContent("strong text 1 "))
    .AddContent(
        new Emphasis()
            .WithTextContent("italic text 1 ")
            .AddContent(
                new Strong()
                    .AddTextContent("strong italic text ")
                    .AddContent(
                        new Strikethrough().AddTextContent("bold strikethrough italic text "))),
        new Strong().AddTextContent("strong text 2 "))
    .AddTextContent("plain text 1");
```

But, there are few limitations due to `fb2-tree` implementation & `c#` not supporting covariant retun types.  
As eagle-eyed readers might have noticed that:

1) Part of `Editing API` implemented in `Fb2Container` class - node related - like `AddContent(Fb2Node node)`, `AddContent(IEnumerable<Fb2Node> nodes)`, `RemoveContent(Fb2Node node)` etc - all have return type of `Fb2Container`.  

```csharp
// So, this will work:
Fb2Container paragraph = new Paragraph().AddContent(() => new Strong().AddTextContent("strong text 1 ")); // or use any other overload of `AddContent`
// This WILL NOT:
Paragraph paragraph = new Paragraph().AddContent(new Strong().AddTextContent("strong text 1 ")); // returns `Fb2Container`, not `Paragraph`
```

2) Part of `Editing API` implemented in `Fb2Element` class - text related - like `AddContent(string newContent, string? separator = null)` or `ClearContent()` etc - all have return type of `Fb2Element`.  

```csharp
// So, this will work:  
Fb2Element plainTextItem = new TextItem().AddContent(() => "text 1 "); // or use any other overload of `AddContent`
// This WILL NOT:
TextItem plainTextItem = new TextItem().AddContent(() => "text 1 "); // returns `Fb2Element`, not `TextItem` 
```

3) Part of `Editing API` implemented in `Fb2Node` class - `Attributes` related - like `AddAttribute(Fb2Attribute attribute)`, `AddAttributes(params Fb2Attribute[] attributes)` or `RemoveAttribute(Fb2Attribute fb2Attribute)` etc - all have return type of `Fb2Node`.  

```csharp
//So, this will work:
Fb2Node paragraph = new Paragraph().AddAttribute(AttributeNames.Id, "testValue"); // but `paragraph` variable has `Fb2Node` type now
// This WILL NOT:
Paragraph paragraph = new Paragraph().AddAttribute(AttributeNames.Id, "testValue"); // can't assign Fb2Node to Paragraph
```
> Attention!
>
> Once `AddAttribute` method (or any overload) is used on `Fb2Element` or `Fb2Container`, returned value is converted to `Fb2Node` type, allowing access for `Attributes` only. This can break method chaining, so it is recommended to use `Attributes`-related methods in last turn in call chain, or to use [Extensions](#extensions).


## Extensions

If you care about what exact type is being returned while editing node, you can use `Fb2ContainerExtensions`, `Fb2ElementExtensions`, `Fb2NodeExtensions` classes from `Fb2.Document.Extensions` namespace.  

Extensions methods naming is slightly differs from naming of appropriate classes, e.g. `AppendContent` extension corresponds to `AddContent`, `DeleteContent` to `RemoveContent` and `EraseContent` to `ClearContent`.

Extensions are generic wrappers around Editing APIs of respective classes - `Fb2Container`, `Fb2Element` and `Fb2Node` - implementations, returning same type of node that was used, without cutting type down to base classes, i.e.:

 This ***will work***: 
```csharp
Paragraph paragraph = new Paragraph().AppendAttribute(AttributeNames.Id, "testValue");
```

Method chaining is also supported:

```csharp
Paragraph paragraph = new Paragraph()
                        .AppendContent(() => Fb2NodeFactory.GetNodeByName(ElementNames.Strong))
                        .AppendContent(ElementNames.Emphasis)
                        .AppendAttribute(AttributeNames.Id, $"{Guid.NewGuid()}")
                        .AppendContent(new Strikethrough().AppendTextContent("Strikethrough", " "))
                        .AppendTextContent("plain text", " ");
```

For more details, check out [Fb2ContainerExtensions](https://github.com/Overrided/Fb2.Document/blob/features/editing_api/Fb2.Document/Extensions/Fb2ContainerExtensions.cs), [Fb2ElementExtensions](https://github.com/Overrided/Fb2.Document/blob/features/editing_api/Fb2.Document/Extensions/Fb2ElementExtensions.cs) and [Fb2NodeExtensions](https://github.com/Overrided/Fb2.Document/blob/features/editing_api/Fb2.Document/Extensions/Fb2NodeExtensions.cs).


## Constants

Most used constants are [ElementNames](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Constants/ElementNames.cs) and [AttributeNames](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Constants/AttributeNames.cs) which contain,
respectively, element and attribute names that are allowed to be loaded from file.

All nodes / attributes, which names are not on list, will be omitted during document loading.


## Error handling

As in fact library operates on top of `XDocument` ([Linq to XML](https://docs.microsoft.com/en-us/dotnet/standard/linq/linq-xml-overview)) requirements on `fb2` content validity from `xml` viewpoint (correctly closed tags etc.) are more than relevant while not forgetting of `fb2 standard` requierements.

To simplify error-handling for different validation, loading and editing errors library provides custom exceptions:

`Fb2DocumentLoadingException`  - thrown if `Fb2Document.Load(...)` or `Fb2Document.LoadAsync(...)` fails.  
`Fb2NodeLoadingException`  - thrown if `Fb2Node.Load(...)` method fails.  
`NoAttributesAllowedException`  - thrown on attempt to add attribute to node with no `AllowedAttributes`.  
`InvalidAttributeException`    - thrown on attempt to add attribute with invalid key/value.  
`UnexpectedAtrributeException`  - thrown on attemt to add attribute not listed in `AllowedAttributes`.  
`InvalidNodeException`  - thrown on attempt to add node to `Fb2Container.Content` using unknown `Fb2Node` name. Also being unhandled by `Fb2NodeFactory.GetNodeByName` method if supplied unknown name.  
`UnexpectedNodeException`  - thrown on attempt to add not allowed node to `Fb2Container.Content` - like to put `plain text` into `BookBody` or try to fit `BodySection` inside `Paragraph`.  

For more examples on exceptions see ['Fb2ContainerTests'](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document.Tests/ModelsTests/Fb2ContainerTests.cs).


## Tests

Solution contains test project, which covers all models configurations & main element loading, validation and generation logic.

As far as project contains 63 different models, which are, in fact, configs for loading of different nodes from files,
it would be incredibly boring to write tests for each of it. Reflection comes as best solution to get all model types, create corresponding instances and try to load
each of them with different sets of data.

Also, there are separate tests for [Fb2ElementFactory](https://github.com/Overrided/Fb2.Document/blob/master/Fb2.Document/Factories/Fb2ElementFactory.cs) and `IntegrationTests` test, which creates `Fb2Document`, saves it as a file, reads it into second model, saves again and checks if both saved files are equal.
