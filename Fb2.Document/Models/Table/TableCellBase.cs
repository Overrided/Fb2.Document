using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public abstract class TableCellBase : TextContainerBase
    {
        public override ImmutableHashSet<string> AllowedAttributes =>
            ImmutableHashSet.Create(
               AttributeNames.Id,
               AttributeNames.ColumnSpan,
               AttributeNames.RowSpan,
               AttributeNames.Align,
               AttributeNames.VerticalAlign,
               AttributeNames.Language);
    }
}
