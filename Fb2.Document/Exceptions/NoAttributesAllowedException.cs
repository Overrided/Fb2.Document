﻿using System;

namespace Fb2.Document.Exceptions
{
    public class NoAttributesAllowedException : Exception
    {
        public string NodeName { get; }

        public NoAttributesAllowedException(string nodeName) : base($"Node '{nodeName}' has no allowed attributes.")
        {
            NodeName = nodeName;
        }
    }
}
