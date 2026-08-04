# PowerShellWixExtension

A Wix Extension for running PowerShell scripts

## NuGet Package

[![NuGet](https://img.shields.io/nuget/v/PowerShellWixExtension.svg?maxAge=2592)](https://www.nuget.org/packages/PowerShellWixExtension/) [![AppVeyor](https://ci.appveyor.com/api/projects/status/github/flcdrg/PowerShellWixExtension?style=plastic)](https://ci.appveyor.com/project/DavidGardiner/powershellwixextension)

All ready to add to an existing Wix project. Grab the latest version from https://www.nuget.org/packages/PowerShellWixExtension/

## Getting Started

1. Add a reference to the PowerShellWixExtension.dll in your Wix Setup Project (NuGet package recommended)
2. Add namespace to .wxs file

```xml
    <?xml version="1.0" encoding="UTF-8"?>
    <Wix xmlns="http://wixtoolset.org/schemas/v4/wxs" xmlns:powershell="http://schemas.gardiner.net.au/PowerShellWixExtensionSchema">
```

4. To execute a .ps1 file that ships with the project

```xml
   <powershell:File Id="PSFile1" File="[#TestPs1]" Arguments="&quot;First Argument&quot; 2"/>
```

5. To execute inline script, use the `Script` attribute with Base64-encoded content

```xml
    <powershell:Script Id="Script2" Script="VwByAGkAdABlAC0ASABvAHMAdAAgACIASABlAGwAbABvIAAoAHcAbwByAGwAZAApACI="/>
```

The script content must be **Base64-encoded UTF-16 (Unicode)** string. To encode a PowerShell script:

```powershell
$script = "Write-Host 'Hello (world)'"
$encoded = [Convert]::ToBase64String([System.Text.Encoding]::Unicode.GetBytes($script))
Write-Host $encoded
```

### Legacy Inner Text Format (WiX 4 / v3)

For backwards compatibility, inline scripts can still use inner text/CDATA (though this may not work with WiX 6+ strict XML validation):

```xml
    <powershell:Script Id="Script2">
      <![CDATA[
        # Write-Host "Number 2";

        for ($i = 1; $i -le 100; $i++) 
        {
          Write-Progress -Activity "Activity" -Status "Status $i% complete" -CurrentOperation "Operation $i" -PercentComplete $i
          Start-Sleep -Milliseconds 5 
        }

        ]]>
    </powershell:Script>
```

**Note**: The compiler will automatically handle both formats, but using the `Script` attribute (Base64-encoded) is recommended for WiX 6+ compatibility.

## Notes

### WiX 6 Migration

This extension has been updated to support WiX 6 with the following changes:

1. **Script elements require Base64-encoded content**: WiX 6 enforces stricter XML schema validation and does not allow inner text on custom extension elements. Use the `Script` attribute with Base64-encoded UTF-16 content instead.

2. **Namespace remains v4**: Despite WiX version 6, the XML namespace remains `http://wixtoolset.org/schemas/v4/wxs` for backwards compatibility.

3. **Platform-specific custom actions**: The underlying PowerShell custom actions use the `Wix4` prefix and platform suffix (`_X86`, `_X64`, `_A64`), following WiX 6 conventions. The extension handles these transparently.

### Custom sequences

You can customise when a set of scripts are run by adding your own `<Custom />` element inside your `<InstallExecuteSequence />` element. eg.

```xml
      <InstallExecuteSequence>
        <Custom Action="PowerShellScriptsDeferred" After="RegisterUser">NOT Installed</Custom>
      </InstallExecuteSequence>
```

The four defined actions are:

1. `PowerShellScriptsDeferred`
2. `PowerShellScriptsElevatedDeferred`
3. `PowerShellFilesDeferred`
4. `PowerShellFilesElevatedDeferred`

### Inline Scripts

* Be aware that if your inline script uses square brackets \[ \], you'll need to escape them like [\\[] [\\]] otherwise they will be interpreted as MSI properties (unless that is what you wanted!)
* When using the `Script` attribute (Base64-encoded), square brackets are automatically protected by encoding
