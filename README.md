PowerShellWixExtension
======================

A Wix Extension for running PowerShell scripts

Getting Started
---------------
1. Add a reference to the PowerShellWixExtension.dll in your Wix Setup Project
2. Add namespace to .wxs file 
`
    <?xml version="1.0" encoding="UTF-8"?>
    <Wix xmlns="http://schemas.microsoft.com/wix/2006/wi" xmlns:powershell="http://schemas.gardiner.net.au/PowerShellWixExtensionSchema">
`

3. To execute a .ps1 file that ships with the project
`
   <powershell:File Id="PSFile1" File="[#TestPs1]" Arguments="&quot;First Argument&quot; 2"/>
`
   
4. To execute inline script use

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
