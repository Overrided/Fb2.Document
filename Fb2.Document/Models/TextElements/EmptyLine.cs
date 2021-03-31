using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Extensions;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class EmptyLine : Fb2Element
    {
        public override string Name => ElementNames.EmptyLine;

        public override void Load([In] XNode element, bool preserveWhitespace = false)
        {
            element.Validate(Name);
            Content = Environment.NewLine;
        }
    }
}
