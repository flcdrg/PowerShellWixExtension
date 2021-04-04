Import-Module Pester

$base = $env:GITHUB_WORKSPACE

if (-not $base) {
    $base = "."
}

Describe 'Inline Scripts' {

    It 'Install' {
        'inlinescript-install.log' | Should -FileContentMatch 'This is an inline script, running non-elevated'
    }
}

Describe 'Scripts' {

    It 'Install' {
        'script-install.log' | Should -FileContentMatch 'This is going to Output'
    }
}