param([string] $first)

$DebugPreference = "Continue"
$VerbosePreference = "Continue"

Write-Host "Testing Test.ps1 - $first"

Write-Output "This is going to Output"

Write-Verbose "This is going to verbose"

Write-Debug  "This is going to Debug"

Write-Host "Current identity"

$wid=[System.Security.Principal.WindowsIdentity]::GetCurrent()
$prp=new-object System.Security.Principal.WindowsPrincipal($wid)
$adm=[System.Security.Principal.WindowsBuiltInRole]::Administrator
    
Write-Host $wid.Name
Write-Host $prp.IsInRole($adm)