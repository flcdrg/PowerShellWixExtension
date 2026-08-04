Import-Module Pester

$base = $env:GITHUB_WORKSPACE

if (-not $base) {
    $base = "."
}

# Check if running as admin (required for MSI installation)
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")

if (-not $isAdmin) {
    Write-Warning "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Warning "Tests require ADMINISTRATOR privileges"
    Write-Warning "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Warning ""
    Write-Warning "MSI installation requires admin rights to:"
    Write-Warning "  • Install to Program Files"
    Write-Warning "  • Write to HKEY_LOCAL_MACHINE registry"
    Write-Warning ""
    Write-Warning "Steps to run tests:"
    Write-Warning "  1. Right-click PowerShell → 'Run as Administrator'"
    Write-Warning "  2. cd D:\git\PowerShellWixExtension"
    Write-Warning "  3. dotnet build PowerShellWixExtension.sln --configuration Release"
    Write-Warning "  4. Run MSI installations (see PESTER_TESTS_ADMIN_REQUIREMENTS.md)"
    Write-Warning "  5. Invoke-Pester -Path .\Tests\Pester.Tests.ps1"
    Write-Warning ""
    Write-Warning "GitHub Actions CI/CD has admin access and tests will pass there."
    Write-Warning "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Warning ""
}

Describe 'Inline Scripts' {

    It 'Install - Script executes and produces output' {
        'inlinescript-install.log' | Should -FileContentMatch 'This is an inline script, running non-elevated'
    }

    It 'Install - Script validates identity management' {
        'inlinescript-install.log' | Should -FileContentMatch 'IsInRole'
    }

    It 'Install - Progress bar is displayed' {
        'inlinescript-install.log' | Should -FileContentMatch 'Activity'
    }

    It 'Uninstall - Log file exists' {
        'inlinescript-uninstall.log' | Should -Exist
    }
}

Describe 'External Script Files' {

    It 'Install - Script file executes successfully' {
        'script-install.log' | Should -FileContentMatch 'This is going to Output'
    }

    It 'Install - First argument is processed' {
        'script-install.log' | Should -FileContentMatch 'Testing Test.ps1'
    }

    It 'Install - Script validates identity' {
        'script-install.log' | Should -FileContentMatch 'Current identity'
    }

    It 'Install - Error handling works (Script4 exit code captured)' {
        'script-install.log' | Should -FileContentMatch 'Exit code'
    }

    It 'Install - Multiple scripts execute in sequence' {
        $logContent = Get-Content 'script-install.log' -Raw
        
        # Verify multiple scripts ran
        $logContent | Should -Match 'This is an inline script, running non-elevated'
        $logContent | Should -Match 'This is going to Output'
    }

    It 'Uninstall - Log file exists' {
        'script-uninstall.log' | Should -Exist
    }
}