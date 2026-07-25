using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using WixToolset.Data;
using WixToolset.Extensibility;

namespace PowerShellWixExtension
{
    public sealed class PowerShellCompilerExtension : BaseCompilerExtension
    {
        public override XNamespace Namespace => "http://schemas.gardiner.net.au/PowerShellWixExtensionSchema";

        public override void ParseElement(Intermediate intermediate, IntermediateSection section, XElement parentElement, XElement element, IDictionary<string, string> contextValues)
        {
            switch (parentElement.Name.LocalName)
            {
                case "Package":
                case "Fragment":
                    switch (element.Name.LocalName)
                    {
                        case "Script":
                            this.ParseScriptElement(intermediate, section, element);
                            break;
                        case "File":
                            this.ParseFileElement(intermediate, section, element);
                            break;
                        default:
                            this.ParseHelper.UnexpectedElement(parentElement, element);
                            break;
                    }

                    break;
                default:
                    this.ParseHelper.UnexpectedElement(parentElement, element);
                    break;
            }
        }

        private void ParseFileElement(Intermediate intermediate, IntermediateSection section, XElement node)
        {
            var sourceLineNumber = this.ParseHelper.GetSourceLineNumbers(node);

            Identifier superElementId = null;
            string file = null;
            string arguments = null;
            string condition = null;
            var elevated = YesNoType.No;
            var ignoreErrors = YesNoType.No;
            var order = 1000000000 + sourceLineNumber.LineNumber;

            foreach (var attribute in node.Attributes())
            {
                if (string.IsNullOrEmpty(attribute.Name.NamespaceName) || this.Namespace == attribute.Name.Namespace)
                {
                    switch (attribute.Name.LocalName)
                    {
                        case "Id":
                            superElementId = this.ParseHelper.GetAttributeIdentifier(sourceLineNumber, attribute);
                            break;
                        case "File":
                            file = this.ParseHelper.GetAttributeValue(sourceLineNumber, attribute);
                            break;
                        case "Arguments":
                            arguments = this.ParseHelper.GetAttributeValue(sourceLineNumber, attribute);
                            break;
                        case "Elevated":
                            elevated = this.ParseHelper.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;
                        case "IgnoreErrors":
                            ignoreErrors = this.ParseHelper.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;
                        case "Order":
                            order = this.ParseHelper.GetAttributeIntegerValue(sourceLineNumber, attribute, 0, 1000000000);
                            break;
                        case "Condition":
                            condition = this.ParseHelper.GetAttributeValue(sourceLineNumber, attribute);
                            break;
                        default:
                            this.ParseHelper.UnexpectedAttribute(node, attribute);
                            break;
                    }
                }
                else
                {
                    this.ParseHelper.ParseExtensionAttribute(this.Context.Extensions, intermediate, section, node, attribute);
                }
            }

            if (superElementId == null)
            {
                this.Messaging.Write(ErrorMessages.ExpectedAttribute(sourceLineNumber, node.Name.LocalName, "Id"));
            }

            if (string.IsNullOrEmpty(file))
            {
                this.Messaging.Write(ErrorMessages.ExpectedAttribute(sourceLineNumber, node.Name.LocalName, "File"));
            }

            if (!this.Messaging.EncounteredError)
            {
                var symbol = this.ParseHelper.CreateSymbol(section, sourceLineNumber, PowerShellSymbolDefinitions.PowerShellFile, superElementId);
                symbol.Set((int)PowerShellFileSymbolFields.File, file);
                symbol.Set((int)PowerShellFileSymbolFields.Arguments, arguments);
                symbol.Set((int)PowerShellFileSymbolFields.Elevated, elevated == YesNoType.Yes ? 1 : 0);
                symbol.Set((int)PowerShellFileSymbolFields.IgnoreErrors, ignoreErrors == YesNoType.Yes ? 1 : 0);
                symbol.Set((int)PowerShellFileSymbolFields.Order, order);
                symbol.Set((int)PowerShellFileSymbolFields.Condition, condition);
            }

            this.ParseHelper.CreateSimpleReference(section, sourceLineNumber, "CustomAction", "PowerShellFilesImmediate");
        }

        private void ParseScriptElement(Intermediate intermediate, IntermediateSection section, XElement node)
        {
            var sourceLineNumber = this.ParseHelper.GetSourceLineNumbers(node);

            Identifier superElementId = null;
            string scriptData = null;
            string condition = null;
            var elevated = YesNoType.No;
            var ignoreErrors = YesNoType.No;
            var order = 1000000000 + sourceLineNumber.LineNumber;

            foreach (var attribute in node.Attributes())
            {
                if (string.IsNullOrEmpty(attribute.Name.NamespaceName) || this.Namespace == attribute.Name.Namespace)
                {
                    switch (attribute.Name.LocalName)
                    {
                        case "Id":
                            superElementId = this.ParseHelper.GetAttributeIdentifier(sourceLineNumber, attribute);
                            break;
                        case "Elevated":
                            elevated = this.ParseHelper.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;
                        case "IgnoreErrors":
                            ignoreErrors = this.ParseHelper.GetAttributeYesNoValue(sourceLineNumber, attribute);
                            break;
                        case "Order":
                            order = this.ParseHelper.GetAttributeIntegerValue(sourceLineNumber, attribute, 0, 1000000000);
                            break;
                        case "Condition":
                            condition = this.ParseHelper.GetAttributeValue(sourceLineNumber, attribute);
                            break;
                        default:
                            this.ParseHelper.UnexpectedAttribute(node, attribute);
                            break;
                    }
                }
                else
                {
                    this.ParseHelper.ParseExtensionAttribute(this.Context.Extensions, intermediate, section, node, attribute);
                }
            }

            var cdata = node.Nodes().OfType<XCData>().FirstOrDefault();
            if (cdata != null)
            {
                scriptData = Convert.ToBase64String(Encoding.Unicode.GetBytes(cdata.Value));
            }
            else if (!string.IsNullOrWhiteSpace(node.Value))
            {
                scriptData = Convert.ToBase64String(Encoding.Unicode.GetBytes(node.Value));
            }

            this.ParseHelper.ParseForExtensionElements(this.Context.Extensions, intermediate, section, node);

            if (superElementId == null)
            {
                this.Messaging.Write(ErrorMessages.ExpectedAttribute(sourceLineNumber, node.Name.LocalName, "Id"));
            }

            if (string.IsNullOrEmpty(scriptData))
            {
                this.Messaging.Write(ErrorMessages.ExpectedElement(sourceLineNumber, node.Name.LocalName, "CDATA"));
            }

            if (!this.Messaging.EncounteredError)
            {
                var symbol = this.ParseHelper.CreateSymbol(section, sourceLineNumber, PowerShellSymbolDefinitions.PowerShellScript, superElementId);
                symbol.Set((int)PowerShellScriptSymbolFields.Script, scriptData);
                symbol.Set((int)PowerShellScriptSymbolFields.Elevated, elevated == YesNoType.Yes ? 1 : 0);
                symbol.Set((int)PowerShellScriptSymbolFields.IgnoreErrors, ignoreErrors == YesNoType.Yes ? 1 : 0);
                symbol.Set((int)PowerShellScriptSymbolFields.Order, order);
                symbol.Set((int)PowerShellScriptSymbolFields.Condition, condition);
            }

            this.ParseHelper.CreateSimpleReference(section, sourceLineNumber, "CustomAction", "PowerShellScriptsImmediate");
        }
    }
}