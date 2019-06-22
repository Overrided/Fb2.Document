using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public abstract class TableCellBase : TextContainer
    {
        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
           AttributeNames.Id,
           AttributeNames.ColumnSpan,
           AttributeNames.RowSpan,
           AttributeNames.Align,
           AttributeNames.VerticalAlign,
           AttributeNames.Language
        };
    }
}
