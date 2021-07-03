using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Tests.DataCollections
{
    public class Fb2ContainerCollection : Fb2NodeCollection, IEnumerable<object[]>
    {
        public override IEnumerator<object[]> GetEnumerator()
        {
            return AllElementsNames
                .Select(name => Fb2NodeFactory.GetNodeByName(name))
                .OfType<Fb2Container>()
                .Select(container => new object[1] { container })
                .GetEnumerator();
        }
    }
}
