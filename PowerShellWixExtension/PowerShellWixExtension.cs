using System;
using System.Collections.Generic;
using WixToolset.Extensibility;

namespace PowerShellWixExtension
{
    public sealed class PowerShellWixExtensionFactory : BaseExtensionFactory
    {
        protected override IReadOnlyCollection<Type> ExtensionTypes => new[]
        {
            typeof(PowerShellCompilerExtension),
            typeof(PowerShellExtensionData),
            typeof(PowerShellWindowsInstallerBackendExtension),
        };
    }
}