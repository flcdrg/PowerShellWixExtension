param([string] $first)

Write-Host "Testing Test.ps1 - $first"

$wid=[System.Security.Principal.WindowsIdentity]::GetCurrent()
$prp=new-object System.Security.Principal.WindowsPrincipal($wid)
$adm=[System.Security.Principal.WindowsBuiltInRole]::Administrator
    
Write-Host $wid.Name
Write-Host $prp.IsInRole($adm)
