using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fb2.Document.Constants;
using Fb2.Documents.TestsV2.Common;
using Xunit;

namespace Fb2.Documents.TestsV2.Fixtures
{
    public class FixtureBase : IAsyncLifetime
    {
        protected ElementNames names = new ElementNames();

        public List<string> AllElementsNames;
        public List<Type> AllModelTypes;

        public virtual Task InitializeAsync()
        {
            AllElementsNames = new List<string>();
            var namesResult = Utils.GetAllFieldsOfType<ElementNames, string>(names);
            AllElementsNames.AddRange(namesResult);


            var assembly = names.GetType().Assembly;
            AllModelTypes = assembly.GetExportedTypes()
                .Where(type => type.FullName.StartsWith("Fb2.Document.Models.") && !type.IsAbstract && type.IsClass)
                .ToList();

            return Task.CompletedTask;
        }

        public virtual Task DisposeAsync()
        {
            AllElementsNames = null;
            AllModelTypes = null;

            return Task.CompletedTask;
        }
    }
}
