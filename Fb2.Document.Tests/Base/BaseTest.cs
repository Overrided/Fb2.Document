using System;
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

        protected const int UnsafeElementsThreshold = 5;

        // Fb2Container creation methods

        public XElement GetXElementWithValidContent(Fb2Container instance)
        {
            if (instance == null)
                throw new ArgumentNullException($"{nameof(instance)} argument is null!");

            XElement resultElement = new XElement(instance.Name);

            foreach (var elementName in (instance as Fb2Container).AllowedElements)
            {
                // EmptyLine will strip any content on loading, so to simpify test we put correct one
                if (elementName == ElementNames.EmptyLine)
                {
                    resultElement.Add(new XElement(elementName));
                    continue;
                }

                XText testText = new XText($"test {elementName} text");

                XElement node = new XElement(elementName);
                node.Add(testText);

                resultElement.Add(node);
            }

            resultElement.Add(new XText("test text"));

            return resultElement;
        }

        public XElement GetXElementWithUnsafeContent(Fb2Container instance)
        {
            if (instance == null)
                throw new ArgumentNullException($"{nameof(instance)} argument is null!");

            var names = new ElementNames();
            var elementNames = Utils.GetAllFieldsOfType<ElementNames, string>(names);

            XElement resultElement = new XElement(instance.Name);

            var unsafeElements = elementNames.Except(instance.AllowedElements);

            foreach (var unsafeElement in unsafeElements.Skip(10).Take(UnsafeElementsThreshold))
            {
                // EmptyLine will strip any content on loading, so to simpify test we put correct one
                if (unsafeElement == ElementNames.EmptyLine)
                {
                    resultElement.Add(new XElement(unsafeElement));
                    continue;
                }

                XText testText = new XText($"test {unsafeElement} text");

                XElement node = new XElement(unsafeElement);
                node.Add(testText);

                resultElement.Add(node);
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

        // Fb2Element creation methods

        public XElement GetXElementWithSimpleStringContent(string elementName)
        {
            var element = new XElement(elementName);

            var content = new XText("simple test text");

            element.Add(content);

            return element;
        }

        public XElement GetXElementWithMultilineStringContent(string elementName)
        {
            var element = new XElement(elementName);

            var content = new XText($"\trow 1{Environment.NewLine}row 2{Environment.NewLine}row 3\t");

            element.Add(content);

            return element;
        }
    }
}
