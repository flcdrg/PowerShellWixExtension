using WixToolset.Data;
using WixToolset.Extensibility;

namespace PowerShellWixExtension
{
    public sealed class PowerShellExtensionData : BaseExtensionData
    {
        public override Intermediate GetLibrary(ISymbolDefinitionCreator symbolDefinitions)
        {
            return Intermediate.Load(typeof(PowerShellExtensionData).Assembly, "PowerShellWixExtension.PowerShellLibrary.wixlib", symbolDefinitions);
        }

        public override bool TryGetSymbolDefinitionByName(string name, out IntermediateSymbolDefinition symbolDefinition)
        {
            symbolDefinition = PowerShellSymbolDefinitions.ByName(name);
            return symbolDefinition != null;
        }
    }
}
