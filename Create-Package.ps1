msbuild /p:Configuration=Release

Write-Host "Expect 1 warning"
.\nuget.exe pack .\PowerShellWixExtension.nuspec -OutputDirectory .\NuGet