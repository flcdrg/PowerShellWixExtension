using System;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Tools.WindowsInstallerXml;

namespace PowerShellWixExtension
{
    public class PowerShellCompilerExtension : CompilerExtension
    {
        private readonly XmlSchema _schema;

        public PowerShellCompilerExtension()
        {
            _schema = LoadXmlSchemaHelper(Assembly.GetExecutingAssembly(), "PowerShellWixExtension.PowerShellWixExtensionSchema.xsd");
        }

        public override XmlSchema Schema
        {
            get
            {
                return _schema;
            }
        }

        public override void ParseElement(SourceLineNumberCollection sourceLineNumbers, XmlElement parentElement, XmlElement element, params string[] contextValues)
        {
            switch (parentElement.LocalName)
            {
                case "Product":
                case "Fragment":
                    switch (element.LocalName)
                    {
                        case "Script":
                            ParseScriptElement(element);
                            break;
                        case "File":
                            ParseFileElement(element);
                            break;
                        default:
                            Core.UnexpectedElement(parentElement, element);
                            break;
                    }

                    break;
                default:
                    Core.UnexpectedElement(
                        parentElement,
                        element);
                    break;
            }
        }

        private void ParseFileElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumber = Preprocessor.GetSourceLineNumbers(node);

            string superElementId = null;
            string file = null;
            string arguments = null;
            var elevated = YesNoType.No;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.NamespaceURI.Length == 0 ||
                    attribute.NamespaceURI == _schema.TargetNamespace)
                {
                    switch (attribute.LocalName)
                    {
                        case "Id":
                            superElementId = Core.GetAttributeIdentifierValue(sourceLineNumber, attribute);
                            break;
                        case "File":
                            file = Core.GetAttributeValue(sourceLineNumber, attribute, false);
                            break;
                        case "Arguments":
                            arguments = Core.GetAttributeValue(sourceLineNumber, attribute);
                            break;
                        case "Elevated":
                            elevated = Core.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;
                        default:
                            Core.UnexpectedAttribute(sourceLineNumber, attribute);
                            break;
                    }
                }
                else
                {
                    Core.UnsupportedExtensionAttribute(sourceLineNumber, attribute);
                }
            }

            if (string.IsNullOrEmpty(superElementId))
            {
                Core.OnMessage(
                    WixErrors.ExpectedAttribute(sourceLineNumber, node.Name, "Id"));
            }

            if (string.IsNullOrEmpty(file))
            {
                Core.OnMessage(
                    WixErrors.ExpectedElement(sourceLineNumber, node.Name, "File"));
            }

            if (!Core.EncounteredError)
            {
                Row superElementRow = Core.CreateRow(sourceLineNumber, "PowerShellFiles");

                superElementRow[0] = superElementId;
                superElementRow[1] = file;
                superElementRow[2] = arguments;
                superElementRow[3] = elevated == YesNoType.Yes ? 1 : 0;
            }

            Core.CreateWixSimpleReferenceRow(sourceLineNumber, "CustomAction", "PowerShellFilesImmediate");
        }

        private void ParseScriptElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumber = Preprocessor.GetSourceLineNumbers(node);

            string superElementId = null;
            string scriptData = null;
            var elevated = YesNoType.No;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.NamespaceURI.Length == 0 ||
                    attribute.NamespaceURI == _schema.TargetNamespace)
                {
                    switch (attribute.LocalName)
                    {
                        case "Id":
                            superElementId = Core.GetAttributeIdentifierValue(sourceLineNumber, attribute);
                            break;
                        case "Elevated":
                            elevated = Core.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;

                        default:
                            Core.UnexpectedAttribute(sourceLineNumber, attribute);
                            break;
                    }
                }
                else
                {
                    Core.UnsupportedExtensionAttribute(sourceLineNumber, attribute);
                }
            }

            if (node.HasChildNodes)
            {
                var cdata = node.ChildNodes[0] as XmlCDataSection;

                if (cdata != null)

                    // Need to encode, as column doesn't like having line feeds
                    scriptData = Convert.ToBase64String(Encoding.Unicode.GetBytes(cdata.Data));
            }

            if (string.IsNullOrEmpty(superElementId))
            {
                Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumber, node.Name, "Id"));
            }

            if (string.IsNullOrEmpty(scriptData))
            {
                Core.OnMessage(WixErrors.ExpectedElement(sourceLineNumber, node.Name, "CDATA"));
            }

            if (!Core.EncounteredError)
            {
                Row superElementRow = Core.CreateRow(sourceLineNumber, "PowerShellScripts");

                superElementRow[0] = superElementId;
                superElementRow[1] = scriptData;
                superElementRow[2] = elevated == YesNoType.Yes ? 1 : 0;
            }

            Core.CreateWixSimpleReferenceRow(sourceLineNumber, "CustomAction", "PowerShellScriptsImmediate");
        }
    }
}