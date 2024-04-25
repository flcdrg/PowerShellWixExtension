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
    <Wix xmlns="http://schemas.microsoft.com/wix/2006/wi" xmlns:powershell="http://schemas.gardiner.net.au/PowerShellWixExtensionSchema">
```

4. To execute a .ps1 file that ships with the project

```xml
   <powershell:File Id="PSFile1" File="[#TestPs1]" Arguments="&quot;First Argument&quot; 2"/>
```

5. To execute inline script use

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

## Notes

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

* Be aware that if your inline script uses square brackets [ ], you'll need to escape them like [\\[] [\\]] otherwise they will be interpreted as MSI properties (unless that is what you wanted!)
