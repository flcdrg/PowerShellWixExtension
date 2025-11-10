using System;
using WixToolset.Data;

namespace PowerShellWixExtension.Symbols
{
    public enum PowerShellSymbolDefinitionType
    {
        PowerShellScripts,
        PowerShellFiles
    }

    public static partial class PowerShellSymbolDefinitions
    {
        public static IntermediateSymbolDefinition ByName(string name)
        {
            if (!Enum.TryParse(name, out PowerShellSymbolDefinitionType type))
            {
                return null;
            }

            return ByType(type);
        }

        public static IntermediateSymbolDefinition ByType(PowerShellSymbolDefinitionType type)
        {
            switch (type)
            {
                case PowerShellSymbolDefinitionType.PowerShellScripts:
                    return PowerShellSymbolDefinitions.PowerShellScripts;

                case PowerShellSymbolDefinitionType.PowerShellFiles:
                    return PowerShellSymbolDefinitions.PowerShellFiles;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}