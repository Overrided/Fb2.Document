using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Tests.DataCollections
{
    public class Fb2ElementCollection : Fb2NodeCollection, IEnumerable<object[]>
    {
        public override IEnumerator<object[]> GetEnumerator() =>
            AllElementsNames
                .Select(name => Fb2NodeFactory.GetNodeByName(name))
                .OfType<Fb2Element>()
                .Select(element => new object[1] { element })
                .GetEnumerator();
    }
}
