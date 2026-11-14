using System;
using WixToolset.Data;

namespace PowerShellWixExtension
{
    public enum PowerShellSymbolDefinitionType
    {
        PowerShellScript,
        PowerShellFile,
    }

    public enum PowerShellScriptSymbolFields
    {
        Script,
        Elevated,
        IgnoreErrors,
        Order,
        Condition,
    }

    public enum PowerShellFileSymbolFields
    {
        File,
        Arguments,
        Elevated,
        IgnoreErrors,
        Order,
        Condition,
    }

    public static class PowerShellSymbolDefinitions
    {
        public static readonly IntermediateSymbolDefinition PowerShellScript = new IntermediateSymbolDefinition(
            PowerShellSymbolDefinitionType.PowerShellScript.ToString(),
            new[]
            {
                new IntermediateFieldDefinition(nameof(PowerShellScriptSymbolFields.Script), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(PowerShellScriptSymbolFields.Elevated), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellScriptSymbolFields.IgnoreErrors), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellScriptSymbolFields.Order), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellScriptSymbolFields.Condition), IntermediateFieldType.String),
            },
            typeof(PowerShellScriptSymbol));

        public static readonly IntermediateSymbolDefinition PowerShellFile = new IntermediateSymbolDefinition(
            PowerShellSymbolDefinitionType.PowerShellFile.ToString(),
            new[]
            {
                new IntermediateFieldDefinition(nameof(PowerShellFileSymbolFields.File), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(PowerShellFileSymbolFields.Arguments), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(PowerShellFileSymbolFields.Elevated), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellFileSymbolFields.IgnoreErrors), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellFileSymbolFields.Order), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellFileSymbolFields.Condition), IntermediateFieldType.String),
            },
            typeof(PowerShellFileSymbol));

        public static bool TryGetSymbolType(string name, out PowerShellSymbolDefinitionType type)
        {
            return Enum.TryParse(name, out type);
        }

        public static IntermediateSymbolDefinition ByName(string name)
        {
            if (!TryGetSymbolType(name, out var type))
            {
                return null;
            }

            return ByType(type);
        }

        public static IntermediateSymbolDefinition ByType(PowerShellSymbolDefinitionType type)
        {
            switch (type)
            {
                case PowerShellSymbolDefinitionType.PowerShellScript:
                    return PowerShellScript;
                case PowerShellSymbolDefinitionType.PowerShellFile:
                    return PowerShellFile;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
