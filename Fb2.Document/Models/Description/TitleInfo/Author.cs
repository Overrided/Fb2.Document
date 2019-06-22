using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Author : CreatorBase
    {
        public override string Name => ElementNames.Author;
    }
}
