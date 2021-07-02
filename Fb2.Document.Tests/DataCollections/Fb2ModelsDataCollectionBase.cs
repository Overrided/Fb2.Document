using System;
using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Tests.Common;
using FluentAssertions;

namespace Fb2.Document.Tests.DataCollections
{
    public abstract class Fb2ModelsDataCollectionBase
    {
        protected ElementNames names = new ElementNames();

        public List<string> AllElementsNames;
        public List<Type> AllModelTypes;

        public Fb2ModelsDataCollectionBase()
        {
            var namesResult = Utils.GetAllFieldsOfType<ElementNames, string>(names);
            AllElementsNames = new List<string>(namesResult);

            var assembly = names.GetType().Assembly;
            AllModelTypes = assembly.GetExportedTypes()
                .Where(type => type.FullName.StartsWith("Fb2.Document.Models.") && !type.IsAbstract && type.IsClass)
                .ToList();

            AllElementsNames.Count.Should().Be(AllModelTypes.Count);
        }
    }
}
