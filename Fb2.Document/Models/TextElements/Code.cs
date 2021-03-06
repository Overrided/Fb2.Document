﻿using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Code : TextContainer
    {
        public override string Name => ElementNames.Code;

        /// <summary>
        /// Specific override to preserve original string content 
        /// with all formatting done with '\t', ' ', '\r\n' etc.
        /// </summary>
        public override void Load(XNode node, bool preserveWhitespace = false)
        {
            base.Load(node, true);
        }
    }
}
