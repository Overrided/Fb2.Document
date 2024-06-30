using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class TextItem : Fb2Element
{
    public override string Name => ElementNames.FictionText;
}
