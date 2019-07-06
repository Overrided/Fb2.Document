﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.Common;

namespace Fb2.Document.Tests.Base
{
    public class BaseTest
    {
        private Utils utils = null;

        public Utils Utils
        {
            get
            {
                if (utils == null)
                    utils = new Utils();

                return utils;
            }
        }

        protected const string InvalidNodeName = "reallyInvalidNodeName";

        protected const string InvalidAttributeName = "reallyInvalidAttributeName";

        protected const string InvalidAttributeValue = "reallyInvalidAttributeValue";

        public XElement GetXElement(Fb2Node instance)
        {
            if (instance == null)
                throw new ArgumentNullException($"{nameof(instance)} argument is null!");

            XElement resultElement = new XElement(instance.Name);

            if (instance.GetType().IsSubclassOf(typeof(Fb2Container)))
            {
                foreach (var elementName in (instance as Fb2Container).AllowedElements)
                {
                    if (elementName == ElementNames.EmptyLine) // EmptyLine with any content is invalid
                    {
                        resultElement.Add(new XElement(elementName));
                        continue;
                    }

                    XText testText = new XText($"test {elementName} text");

                    XElement node = new XElement(elementName);
                    node.Add(testText);

                    resultElement.Add(node);
                }
            }

            return resultElement;
        }

        public XElement GetXElementWithAttributes(Fb2Node instance)
        {
            if (instance.AllowedAttributes == null || !instance.AllowedAttributes.Any())
                return new XElement(instance.Name);

            List<XAttribute> attrs = instance.AllowedAttributes.Select(aa => new XAttribute(aa, $"{aa} test value")).ToList();

            XElement resultElement = new XElement(instance.Name, attrs);

            return resultElement;
        }

        public XElement GetXElementWithInvalidContent(string elementName)
        {
            XElement resultElement = new XElement(elementName);

            var invalidNode = new XElement(InvalidNodeName);

            resultElement.Add(invalidNode);

            return resultElement;
        }

        public XElement GetXElementWithInvalidAttribute(string elementName)
        {
            XElement resultElement = new XElement(elementName, new XAttribute(InvalidAttributeName, InvalidAttributeValue));

            return resultElement;
        }
    }
}
