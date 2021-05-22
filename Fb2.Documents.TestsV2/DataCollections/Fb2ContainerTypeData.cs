using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Models.Base;

namespace Fb2.Documents.TestsV2.DataCollections
{
    public class Fb2ContainerTypeData : Fb2ModelsDataCollectionBase, IEnumerable<object[]>
    {
        public List<object[]> ContainerModelTypes = new List<object[]>();

        public Fb2ContainerTypeData()
        {
            var elementTypes = AllModelTypes
                .Where(ct => ct.IsSubclassOf(typeof(Fb2Container)) && !ct.IsAbstract)
                .Select(t => new object[1] { t }); // one model type at a time, right? :)

            ContainerModelTypes.AddRange(elementTypes);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return ContainerModelTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
