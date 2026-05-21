using System.Collections.Generic;
using System.Linq;

namespace Fb2.Document.Tests.DataCollections
{
    public class Fb2NodeNameCollection : Fb2NodeCollection
    {
        public override IEnumerator<object[]> GetEnumerator() =>
            AllElementsNames
                .Select(elName => new object[1] { elName })
                .GetEnumerator();
    }
}
