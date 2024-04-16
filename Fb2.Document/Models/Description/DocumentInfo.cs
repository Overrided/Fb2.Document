using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class DocumentInfo : Fb2Container
{
    public override string Name => ElementNames.DocumentInfo;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedElements =>
        ImmutableHashSet.Create(
            ElementNames.Author,
            ElementNames.ProgramUsed,
            ElementNames.Date,
            ElementNames.SrcUrl,
            ElementNames.SrcOcr,
            ElementNames.FictionId,
            ElementNames.Version,
            ElementNames.History,
            ElementNames.Publisher);
}
