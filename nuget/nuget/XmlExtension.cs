using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Gehtsoft.Build.Nuget
{
    public static class XmlExtension
    {
        #region IO

        public static void LoadFile(this XmlDocument document, string file, XmlSchema schema = null)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                document.LoadFile(fs, schema);
            }
        }

        public static void LoadFile(this XmlDocument document, Stream fs, XmlSchema schema = null)
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Document,
                IgnoreComments = true,
            };

            if (schema != null)
                readerSettings.ValidationType = ValidationType.Schema;

            using (XmlReader reader = XmlReader.Create(fs, readerSettings))
            {

                document.Load(reader);

                if (schema != null)
                {
                    document.Schemas.Add(schema);
                    document.Validate((o, a) => throw a.Exception);
                }
            }
        }

        #endregion

        #region Creation
        public static XmlElement CreateRoot(this XmlDocument document, string name)
        {
            XmlElement element = document.CreateElement(name);
            document.AppendChild(element);
            return element;
        }
        public static XmlElement CreateRoot(this XmlDocument document, string name, string prefix, string nameSpaceUri)
        {
            XmlElement element = document.CreateElement(prefix, name, nameSpaceUri);
            document.AppendChild(element);
            return element;
        }

        public static XmlElement AddElement(this XmlElement parent, string name)
        {
            XmlElement element = parent.OwnerDocument.CreateElement(name);
            parent.AppendChild(element);
            return element;
        }
        public static XmlElement AddElement(this XmlElement parent, string name, string prefix, string nameSpaceUri)
        {
            XmlElement element;
            if (!string.IsNullOrEmpty(nameSpaceUri))
                element = parent.OwnerDocument.CreateElement(prefix ?? String.Empty, name, nameSpaceUri);
            else
                element = parent.OwnerDocument.CreateElement(name);
            parent.AppendChild(element);
            return element;
        }

        public static XmlElement AddAttribute(this XmlElement element, string name, string value)
        {
            if (element.HasAttribute(name))
                element.Attributes[name].Value = value;
            else
            {
                XmlAttribute attribute = element.OwnerDocument?.CreateAttribute(name);
                attribute.Value = value;
                element.Attributes.Append(attribute);
            }

            return element;
        }

        public static XmlElement ChildOrCreate(this XmlElement element, string name, string prefix, string namespaceUri)
        {
            XmlElement child = element.Children(name, namespaceUri).FirstOrDefault();
            if (child == null)
                child = element.AddElement(name, prefix, namespaceUri);
            return child;
        }
        #endregion


        #region Navigation
        public static string Attribute(this XmlElement element, string name)
        {
            if (element?.Attributes == null)
                return null;
            if (!element.HasAttribute(name))
                return null;
            return element.Attributes[name].Value;
        }

        public static string AttributeOrElement(this XmlElement element, string name)
        {
            string v = element?.Attribute(name);
            if (!string.IsNullOrEmpty(v))
                return v;
            XmlElement e = element?.Children(name)?.FirstOrDefault();
            return e?.InnerText;
        }

        public static IEnumerable<string> Attribute(this IEnumerable<XmlElement> elements, string name)
        {
            if (elements == null)
                yield break;
            foreach (XmlElement element in elements)
            {
                string attribute = element.Attribute(name);
                if (!string.IsNullOrEmpty(attribute))
                    yield return attribute;
            }
        }

        public static IEnumerable<XmlElement> Children(this XmlElement element, string name, string namespaceUri)
        {
            if (element == null)
                yield break;
            if (element.ChildNodes == null || element.ChildNodes.Count == 0)
                yield break;

            foreach (XmlNode node in element.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element && 
                    (string.IsNullOrEmpty(name) || node.LocalName == name) &&
                    (string.IsNullOrEmpty(namespaceUri) || node.NamespaceURI == namespaceUri))
                    yield return (XmlElement)node;
            }
        }

        public static IEnumerable<XmlElement> Descendants(this XmlElement element, string name, string namespaceUri)
        {
            if (element == null)
                yield break;
            if (element.ChildNodes == null || element.ChildNodes.Count == 0)
                yield break;

            foreach (XmlNode node in element.ChildNodes)
            {

                if (node.NodeType == XmlNodeType.Element)
                { 
                    XmlElement child = (XmlElement) node;
                    foreach (XmlElement child1 in child.Descendants(name, namespaceUri))
                        yield return child1;

                    if ((string.IsNullOrEmpty(name) || node.LocalName == name) && (string.IsNullOrEmpty(namespaceUri) || node.NamespaceURI == namespaceUri))
                        yield return child;
                }
            }
        }
        public static IEnumerable<XmlElement> Children(this XmlElement element, string name) => Children(element, name, null);
        public static IEnumerable<XmlElement> Children(this XmlElement element) => Children(element, null, null);
        public static IEnumerable<XmlElement> Descendants(this XmlElement element, string name) => Descendants(element, name, null);
        public static IEnumerable<XmlElement> Descendants(this XmlElement element) => Descendants(element, null, null);
        public static IEnumerable<XmlElement> Children(this XmlDocument document, string name, string namespaceUri)
        {
            XmlElement element = document.DocumentElement;
            if (element == null)
                yield break;
            if ((string.IsNullOrEmpty(name) || element.LocalName == name) &&
                (string.IsNullOrEmpty(namespaceUri) || element.NamespaceURI == namespaceUri))
                yield return element;

        }

        public static IEnumerable<XmlElement> Descendants(this XmlDocument document, string name, string namespaceUri)
        {
            XmlElement element = document.DocumentElement;
            if (element == null)
                yield break;

            if ((string.IsNullOrEmpty(name) || element.LocalName == name) &&
                (string.IsNullOrEmpty(namespaceUri) || element.NamespaceURI == namespaceUri))
                yield return element;

            foreach (XmlElement child in element.Descendants(name, namespaceUri))
                yield return child;

        }
        public static IEnumerable<XmlElement> Children(this XmlDocument document, string name) => Children(document, name, null);
        public static IEnumerable<XmlElement> Children(this XmlDocument document) => Children(document, null, null);
        public static IEnumerable<XmlElement> Descendants(this XmlDocument document, string name) => Descendants(document, name, null);
        public static IEnumerable<XmlElement> Descendants(this XmlDocument document) => Descendants(document, null, null);

        public static IEnumerable<XmlElement> Children(this IEnumerable<XmlElement> elements, string name, string namespaceUri)
        {
            if (elements == null)
                yield break;
            foreach (XmlElement element in elements)
            foreach (XmlElement element1 in element.Children(name, namespaceUri))
                yield return element1;
        }

        public static IEnumerable<XmlElement> Descendants(this IEnumerable<XmlElement> elements, string name, string namespaceUri)
        {
            if (elements == null)
                yield break;
            foreach (XmlElement element in elements)
            foreach (XmlElement element1 in element.Descendants(name, namespaceUri))
                yield return element1;

        }

        public static IEnumerable<XmlElement> Children(this IEnumerable<XmlElement> element, string name) => Children(element, name, null);
        public static IEnumerable<XmlElement> Children(this IEnumerable<XmlElement> element) => Children(element, null, null);
        public static IEnumerable<XmlElement> Descendants(this IEnumerable<XmlElement> element, string name) => Descendants(element, name, null);
        public static IEnumerable<XmlElement> Descendants(this IEnumerable<XmlElement> element) => Descendants(element, null, null);
        #endregion
    }
}
