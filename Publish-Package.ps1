# Get latest nupkg

$nupkg = Get-ChildItem .\Nuget\*.nupkg | Sort-Object -Descending -Property LastWriteTime | Select-Object -First 1

.\NuGet Push $nupkg -source nuget.org