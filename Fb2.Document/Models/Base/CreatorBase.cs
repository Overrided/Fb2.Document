using System.Collections.Immutable;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base;

public abstract class CreatorBase : Fb2Container
{
    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedElements =>
    [
        ElementNames.FirstName,
        ElementNames.MiddleName,
        ElementNames.LastName,
        ElementNames.NickName,
        ElementNames.Email,
        ElementNames.HomePage,
        ElementNames.FictionId
    ];
}
