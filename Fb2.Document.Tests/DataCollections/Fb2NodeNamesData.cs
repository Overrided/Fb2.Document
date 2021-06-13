using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fb2.Document.Tests.DataCollections
{
    public class Fb2NodeNamesData : Fb2ModelsDataCollectionBase, IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            return AllElementsNames.Select(name => new object[1] { name }).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
