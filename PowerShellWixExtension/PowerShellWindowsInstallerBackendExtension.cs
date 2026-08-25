using System.Collections.Generic;
using WixToolset.Data;
using WixToolset.Data.WindowsInstaller;
using WixToolset.Extensibility;

namespace PowerShellWixExtension
{
    public sealed class PowerShellWindowsInstallerBackendExtension : BaseWindowsInstallerBackendBinderExtension
    {
        public override IReadOnlyCollection<TableDefinition> TableDefinitions => PowerShellTableDefinitions.All;

        public override bool TryProcessSymbol(IntermediateSection section, IntermediateSymbol symbol, WindowsInstallerData output, TableDefinitionCollection tableDefinitions)
        {
            if (!PowerShellSymbolDefinitions.TryGetSymbolType(symbol.Definition.Name, out var symbolType))
            {
                return base.TryProcessSymbol(section, symbol, output, tableDefinitions);
            }

            switch (symbolType)
            {
                case PowerShellSymbolDefinitionType.PowerShellScript:
                {
                    var row = this.BackendHelper.CreateRow(section, symbol, output, PowerShellTableDefinitions.PowerShellScripts);
                    row[0] = symbol.Id.Id;
                    row[1] = symbol[(int)PowerShellScriptSymbolFields.Script].AsString();
                    row[2] = symbol[(int)PowerShellScriptSymbolFields.Elevated].AsNumber();
                    row[3] = symbol[(int)PowerShellScriptSymbolFields.IgnoreErrors].AsNumber();
                    row[4] = symbol[(int)PowerShellScriptSymbolFields.Order].AsNumber();
                    row[5] = symbol[(int)PowerShellScriptSymbolFields.Condition].AsString();
                    return true;
                }

                case PowerShellSymbolDefinitionType.PowerShellFile:
                {
                    var row = this.BackendHelper.CreateRow(section, symbol, output, PowerShellTableDefinitions.PowerShellFiles);
                    row[0] = symbol.Id.Id;
                    row[1] = symbol[(int)PowerShellFileSymbolFields.File].AsString();
                    row[2] = symbol[(int)PowerShellFileSymbolFields.Arguments].AsString();
                    row[3] = symbol[(int)PowerShellFileSymbolFields.Elevated].AsNumber();
                    row[4] = symbol[(int)PowerShellFileSymbolFields.IgnoreErrors].AsNumber();
                    row[5] = symbol[(int)PowerShellFileSymbolFields.Order].AsNumber();
                    row[6] = symbol[(int)PowerShellFileSymbolFields.Condition].AsString();
                    return true;
                }

                default:
                    return base.TryProcessSymbol(section, symbol, output, tableDefinitions);
            }
        }
    }
}
