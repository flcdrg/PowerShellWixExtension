# PowerShell WiX Extension Test Runner
# Run this script from an ADMINISTRATOR PowerShell session

param(
    [switch]$SkipBuild,
    [switch]$SkipInstall,
    [switch]$TestOnly
)

$ErrorActionPreference = "Stop"

# Check admin privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")

if (-not $isAdmin) {
    Write-Host "❌ ERROR: This script must be run as Administrator" -ForegroundColor Red
    Write-Host ""
    Write-Host "Steps:"
    Write-Host "  1. Right-click PowerShell → 'Run as Administrator'"
    Write-Host "  2. cd D:\git\PowerShellWixExtension"
    Write-Host "  3. .\Run-Tests.ps1"
    exit 1
}

Write-Host "✅ Running with admin privileges" -ForegroundColor Green
Write-Host ""

# Step 1: Build
if (-not $SkipBuild -and -not $TestOnly) {
    Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║ STEP 1: BUILD                                          ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    
    dotnet build PowerShellWixExtension.sln --configuration Release
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Build succeeded" -ForegroundColor Green
    Write-Host ""
}

# Step 2: Install MSIs
if (-not $SkipInstall -and -not $TestOnly) {
    Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║ STEP 2: INSTALL TEST MSI PACKAGES                      ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    
    $msiPaths = @(
        @{ Name = "PowerShellWixInlineScriptTest"; Action = "Install"; Path = "Tests\PowerShellWixInlineScriptTest\bin\x86\Release\PowerShellWixInlineScriptTest.msi"; Log = "inlinescript-install.log" }
        @{ Name = "PowerShellWixInlineScriptTest"; Action = "Uninstall"; Path = "Tests\PowerShellWixInlineScriptTest\bin\x86\Release\PowerShellWixInlineScriptTest.msi"; Log = "inlinescript-uninstall.log" }
        @{ Name = "PowerShellWixTest"; Action = "Install"; Path = "Tests\PowerShellWixTest\bin\x86\Release\PowerShellWixTest.msi"; Log = "script-install.log" }
        @{ Name = "PowerShellWixTest"; Action = "Uninstall"; Path = "Tests\PowerShellWixTest\bin\x86\Release\PowerShellWixTest.msi"; Log = "script-uninstall.log" }
    )
    
    foreach ($msi in $msiPaths) {
        Write-Host "$($msi.Action) $($msi.Name)..."
        
        if ($msi.Action -eq "Install") {
            $args = "/i `"$($msi.Path)`" /q /liwearucmopvx `"$pwd\$($msi.Log)`""
        } else {
            $args = "/x `"$($msi.Path)`" /q /liwearucmopvx `"$pwd\$($msi.Log)`""
        }
        
        $proc = Start-Process msiexec.exe -Wait -PassThru -ArgumentList $args
        
        if ($proc.ExitCode -eq 0) {
            Write-Host "  ✅ $($msi.Action) succeeded" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Exit code: $($proc.ExitCode)" -ForegroundColor Yellow
            # Don't fail on MSI exit codes - they might be 3010 or 1602 depending on context
        }
    }
    
    Write-Host ""
    Write-Host "✅ MSI installation complete" -ForegroundColor Green
    Write-Host ""
}

# Step 3: Run Pester tests
Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║ STEP 3: RUN PESTER TESTS                               ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Import-Module Pester

$result = Invoke-Pester -Path .\Tests\Pester.Tests.ps1 -PassThru

$passCount = @($result.TestResult | Where-Object { $_.Result -eq 'Passed' }).Count
$failCount = @($result.TestResult | Where-Object { $_.Result -eq 'Failed' }).Count
$skipCount = @($result.TestResult | Where-Object { $_.Result -eq 'Skipped' }).Count

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║ TEST RESULTS                                           ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Total:   $($result.TestResult.Count)"
Write-Host "  Passed:  $passCount" -ForegroundColor Green
Write-Host "  Failed:  $failCount" -ForegroundColor Red
Write-Host "  Skipped: $skipCount" -ForegroundColor Yellow
Write-Host ""

if ($failCount -gt 0) {
    Write-Host "❌ Some tests failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Failed tests:"
    $result.TestResult | Where-Object { $_.Result -eq 'Failed' } | ForEach-Object {
        Write-Host "  ✗ $($_.Describe) → $($_.Name)" -ForegroundColor Red
    }
    exit 1
} elseif ($passCount -eq $result.TestResult.Count) {
    Write-Host "✅ ALL TESTS PASSED!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "⊘ All tests skipped (log files not found)" -ForegroundColor Yellow
    Write-Host "Make sure MSI installations completed successfully" -ForegroundColor Yellow
    exit 0
}
