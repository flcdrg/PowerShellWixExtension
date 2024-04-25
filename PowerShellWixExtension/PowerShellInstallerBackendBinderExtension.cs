using System.Collections.Generic;

using WixToolset.Data.WindowsInstaller;
using WixToolset.Extensibility;

namespace PowerShellWixExtension
{
    public class PowerShellInstallerBackendBinderExtension : BaseWindowsInstallerBackendBinderExtension
    {
        public override IReadOnlyCollection<TableDefinition> TableDefinitions => PowerShellTableDefinitions.All;
    }
}