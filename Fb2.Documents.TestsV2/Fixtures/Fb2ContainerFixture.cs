using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fb2.Document.Models.Base;
using Xunit;

namespace Fb2.Documents.TestsV2.Fixtures
{
    public class Fb2ContainerFixture : FixtureBase
    {
        public List<Type> ContainerModelTypes;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            ContainerModelTypes = AllModelTypes
                .Where(ct => ct.IsSubclassOf(typeof(Fb2Container)))
                .ToList();
        }

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync();

            ContainerModelTypes = null;
        }
    }

    [CollectionDefinition(nameof(Fb2ContainerFixtureCollection))]
    public class Fb2ContainerFixtureCollection : ICollectionFixture<Fb2ContainerFixture>
    {
    }
}
