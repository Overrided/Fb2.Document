using System.Collections.Immutable;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class TableRow : Fb2Container
    {
        public override string Name => ElementNames.TableRow;

        public override bool CanContainText => false;

        public override ImmutableHashSet<string> AllowedAttributes => ImmutableHashSet.Create(AttributeNames.Align);

        public override ImmutableHashSet<string> AllowedElements =>
            ImmutableHashSet.Create(ElementNames.TableHeader, ElementNames.TableCell);

        public sealed override string ToString() => string.Join(" ", Content().Select(cell => $"{cell}  |"));
    }
}
