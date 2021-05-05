using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Extensions;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    // TODO : CHECK HOW TO PREVENT TINKERING WITH CONTENT HERE
    public class EmptyLine : Fb2Element
    {
        public override string Name => ElementNames.EmptyLine;

        public sealed override void Load([In] XNode element, bool preserveWhitespace = false)
        {
            element.Validate(Name);
            Content = Environment.NewLine;
        }

        // empty line has no content...only purpose )
        //public override Fb2Element WithContent(string content, bool preserveWhitespace = false) => this;
    }
}
