using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class TableRow : Fb2Container
    {
        public override string Name => ElementNames.TableRow;

        public override bool CanContainText => false;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Align
        };

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.TableHeader,
            ElementNames.TableCell
        };

        public sealed override string ToString() => string.Join(" ", Content.Select(cell => $"{cell}  |"));
    }
}
