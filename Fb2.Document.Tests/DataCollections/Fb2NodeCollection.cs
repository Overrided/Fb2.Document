using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.Common;
using FluentAssertions;

namespace Fb2.Document.Tests.DataCollections;

public class Fb2NodeCollection : IEnumerable<object[]>
{
    protected ElementNames names = new ElementNames();

    public List<string> AllElementsNames;
    public List<Type> AllModelTypes;

    public Fb2NodeCollection()
    {
        var namesResult = Utils.GetAllFieldsOfType<ElementNames, string>(names);
        AllElementsNames = new List<string>(namesResult);

        var assembly = names.GetType().Assembly;
        AllModelTypes = assembly.GetExportedTypes()
            .Where(type => type.FullName.StartsWith("Fb2.Document.Models.") &&
                               !type.IsAbstract &&
                               type.IsClass &&
                               type.IsSubclassOf(typeof(Fb2Node)))
            .ToList();

        AllElementsNames.Count.Should().Be(AllModelTypes.Count);
    }

    public virtual IEnumerator<object[]> GetEnumerator()
    {
        return AllElementsNames
            .Select(name => new object[1] { Fb2NodeFactory.GetNodeByName(name) })
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
