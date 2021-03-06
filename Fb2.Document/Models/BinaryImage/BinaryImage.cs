﻿using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    // Nevermind whitespaces on String.Join in ToString() in base class, whitespaces are ignored while decoding from base64
    public class BinaryImage : Fb2Element
    {
        public override string Name => ElementNames.BinaryImage;

        public override bool IsInline => false;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.ContentType,
            AttributeNames.Id
        };
    }
}
