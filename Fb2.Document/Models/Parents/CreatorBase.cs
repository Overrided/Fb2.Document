using System.Collections.Generic;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base
{
    public abstract class CreatorBase : Fb2Container
    {
        public override bool CanContainText => false;

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.FirstName,
            ElementNames.MiddleName,
            ElementNames.LastName,
            ElementNames.NickName,
            ElementNames.Email,
            ElementNames.HomePage,
            ElementNames.FictionId
        };
    }
}
