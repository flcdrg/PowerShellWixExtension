using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using WixToolset.Data;
using WixToolset.Data.WindowsInstaller;
using WixToolset.Extensibility;
using WixToolset.Extensibility.Data;

namespace PowerShellWixExtension
{
    public class PowerShellCompilerExtension : BaseCompilerExtension
    {
        //private readonly XmlSchema _schema;

        //public PowerShellCompilerExtension()
        //{
        //    _schema = LoadXmlSchemaHelper(Assembly.GetExecutingAssembly(), "PowerShellWixExtension.PowerShellWixExtensionSchema.xsd");
        //}

        //public override XmlSchema Schema
        //{
        //    get
        //    {
        //        return _schema;
        //    }
        //}

        public override void ParseElement(Intermediate intermediate, IntermediateSection section, XElement parentElement, XElement element,
            IDictionary<string, string> context)
        {
            switch (parentElement.Name.LocalName)
            {
                case "Product":
                case "Fragment":
                    switch (element.Name.LocalName)
                    {
                        case "Script":
                            ParseScriptElement(element);
                            break;
                        case "File":
                            ParseFileElement(element, section);
                            break;
                        default:
                            ParseHelper.UnexpectedElement(parentElement, element);
                            break;
                    }

                    break;
                default:
                    ParseHelper.UnexpectedElement(
                        parentElement,
                        element);
                    break;
            }

        }

        private void ParseFileElement(XElement node, IntermediateSection section)
        {
            var sourceLineNumber = ParseHelper.GetSourceLineNumbers(node);

            string superElementId = null;
            string file = null;
            string arguments = null;
            string condition = null;
            var elevated = YesNoType.No;
            YesNoType ignoreErrors = YesNoType.No;
            var order = 1000000000 + sourceLineNumber.LineNumber;

            foreach (var attribute in node.Attributes())
            {
                if (attribute.Name.NamespaceName.Length == 0 ||
                    attribute.Name.Namespace == Namespace)
                {
                    switch (attribute.Name.LocalName)
                    {
                        case "Id":
                            superElementId = ParseHelper.GetAttributeIdentifierValue(sourceLineNumber, attribute);
                            break;
                        case "File":
                            file = ParseHelper.GetAttributeValue(sourceLineNumber, attribute);
                            break;
                        case "Arguments":
                            arguments = ParseHelper.GetAttributeValue(sourceLineNumber, attribute);
                            break;
                        case "Elevated":
                            elevated = ParseHelper.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;
                        case "IgnoreErrors":
                            ignoreErrors = ParseHelper.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;
                        case "Order":
                            order = ParseHelper.GetAttributeIntegerValue(sourceLineNumber, attribute, 0, 1000000000);
                            break;
                        case "Condition":
                            condition = ParseHelper.GetAttributeValue(sourceLineNumber, attribute);
                            break;
                        default:
                            ParseHelper.UnexpectedAttribute(node, attribute);
                            break;
                    }
                }
                else
                {
                    Messaging.Write(ErrorMessages.UnsupportedExtensionAttribute(sourceLineNumber, attribute.Parent.Name.LocalName, attribute.Name.LocalName));
                }
            }

            if (string.IsNullOrEmpty(superElementId))
            {
                Messaging.Write(ErrorMessages.ExpectedAttribute(sourceLineNumber, node.Name.LocalName, "Id"));
            }

            if (string.IsNullOrEmpty(file))
            {
                Messaging.Write(ErrorMessages.ExpectedElement(sourceLineNumber, "File"));
            }

            if (order == null)
            {
                Messaging.Write(ErrorMessages.ExpectedAttribute(sourceLineNumber, node.Name.LocalName, "Order"));
            }

            if (!Messaging.EncounteredError)
            {
                Row superElementRow = Core.CreateRow(sourceLineNumber, "PowerShellFiles");

                superElementRow[0] = superElementId;
                superElementRow[1] = file;
                superElementRow[2] = arguments;
                superElementRow[3] = elevated == YesNoType.Yes ? 1 : 0;
                superElementRow[4] = (ignoreErrors == YesNoType.Yes) ? 1 : 0;
                superElementRow[5] = order;
                superElementRow[6] = condition;
            }

            ParseHelper.CreateCustomActionReference(sourceLineNumber, section, "PowerShellFilesImmediate", Context.Platform, CustomActionPlatforms.X86 | CustomActionPlatforms.X64 | CustomActionPlatforms.ARM64);
        }

        private void ParseScriptElement(XElement node)
        {
            SourceLineNumberCollection sourceLineNumber = Preprocessor.GetSourceLineNumbers(node);

            string superElementId = null;
            string scriptData = null;
            string condition = null;
            var elevated = YesNoType.No;
            YesNoType ignoreErrors = YesNoType.No;
            int order = 1000000000 + sourceLineNumber[0].LineNumber;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.NamespaceURI.Length == 0 ||
                    attribute.NamespaceURI == _schema.TargetNamespace)
                {
                    switch (attribute.LocalName)
                    {
                        case "Id":
                            superElementId = ParseHelper.GetAttributeIdentifierValue(sourceLineNumber, attribute);
                            break;
                        case "Elevated":
                            elevated = ParseHelper.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;
                        case "IgnoreErrors":
                            ignoreErrors = ParseHelper.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;
                        case "Order":
                            order = ParseHelper.GetAttributeIntegerValue(sourceLineNumber, attribute, 0, 1000000000);
                            break;
                        case "Condition":
                            condition = ParseHelper.GetAttributeValue(sourceLineNumber, attribute);
                            break;

                        default:
                            ParseHelper.UnexpectedAttribute(sourceLineNumber, attribute);
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
                superElementRow[3] = (ignoreErrors == YesNoType.Yes) ? 1 : 0;
                superElementRow[4] = order;
                superElementRow[5] = condition;
            }

            Core.CreateWixSimpleReferenceRow(sourceLineNumber, "CustomAction", "PowerShellScriptsImmediate");
        }

        public override XNamespace Namespace => "http://schemas.gardiner.net.au/PowerShellWixExtensionSchema";
    }
}