Import-Module Pester

$base = $env:GITHUB_WORKSPACE

if (-not $base) {
    $base = "."
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