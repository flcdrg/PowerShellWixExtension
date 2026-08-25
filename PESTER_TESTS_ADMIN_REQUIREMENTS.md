# Pester Tests - Admin Privileges Requirement

## Issue Summary

The Pester tests are failing during CI/CD pipeline execution or local test runs because the MSI installation fails with error code 1603 and error message:

```
Error 1925: You do not have sufficient privileges to complete this installation 
for all users of the machine. Log on as administrator and then retry this installation.
```

## Root Cause

Windows Installer (msiexec.exe) requires administrator privileges when:
- Installing to `Program Files` or `Program Files (x86)`
- Writing to HKEY_LOCAL_MACHINE registry
- Installing for "all users" vs "current user"

The test MSI packages install to `C:\Program Files (x86)\PowerShellWixTest\`, which requires admin rights.

## Impact on Tests

Without successful MSI installation:
1. PowerShell scripts embedded in MSI never execute
2. MSI log files don't contain expected script output:
   - ❌ "This is an inline script, running non-elevated"
   - ❌ "This is going to Output"
   - ❌ "Current identity"
   - ❌ Progress bar output
3. Pester assertions fail because expected strings are not found in logs

## Solution Options

### Option 1: Run with Administrator Privileges (RECOMMENDED)

**Locally:**
```powershell
# Run PowerShell as Administrator, then:
cd D:\git\PowerShellWixExtension

# Build
dotnet build PowerShellWixExtension.sln --configuration Release

# Run MSI installations
Start-Process msiexec.exe -Wait -ArgumentList "/i Tests\PowerShellWixInlineScriptTest\bin\x86\Release\PowerShellWixInlineScriptTest.msi /q /liwearucmopvx inlinescript-install.log"
Start-Process msiexec.exe -Wait -ArgumentList "/x Tests\PowerShellWixInlineScriptTest\bin\x86\Release\PowerShellWixInlineScriptTest.msi /q /liwearucmopvx inlinescript-uninstall.log"
Start-Process msiexec.exe -Wait -ArgumentList "/i Tests\PowerShellWixTest\bin\x86\Release\PowerShellWixTest.msi /q /liwearucmopvx script-install.log"
Start-Process msiexec.exe -Wait -ArgumentList "/x Tests\PowerShellWixTest\bin\x86\Release\PowerShellWixTest.msi /q /liwearucmopvx script-uninstall.log"

# Run tests
Invoke-Pester -Path .\Tests\Pester.Tests.ps1
```

**In CI/CD (GitHub Actions):**

The workflow already runs on `windows-latest` runner, which has admin access. The current workflow should work fine:
```yaml
- name: msiexec
  run: |
    Start-Process msiexec.exe -Wait -ArgumentList "/i Tests\PowerShellWixInlineScriptTest\bin\x86\Release\PowerShellWixInlineScriptTest.msi /q /liwearucmopvx ${{ github.workspace }}\inlinescript-install.log"
    # ... (other MSI installs)
```

### Option 2: Modify MSI to Install Per-User (Not Recommended)

Change the WiX `Package` element to use per-user installation:
```xml
<Package InstallScope="perUser" ... />
```

**Drawbacks:**
- Changes installation scope/behavior
- May not work for all users
- Not appropriate for system-wide extension

### Option 3: Skip Tests Without Admin Rights (For Local Development)

Modify Pester tests to skip gracefully when admin rights aren't available:

```powershell
BeforeAll {
    $admin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
    if (-not $admin) {
        Write-Warning "Tests require administrator privileges. Skipping MSI-dependent tests."
    }
}

Describe 'Inline Scripts' -Skip:(-not $admin) {
    It 'Install - Script executes and produces output' {
        # ... test code
    }
}
```

### Option 4: Use Test-Only Installation (Most Complex)

Create alternative test MSI that installs to a user-writable location (e.g., AppData), but this defeats the purpose of testing real-world installation behavior.

## Recommended Approach

**For Local Development:**
- Run PowerShell as Administrator before running tests
- OR: Use Option 3 (skip tests without admin) for convenience development cycles

**For CI/CD:**
- Use Option 1 (default)
- GitHub Actions runners have admin access by default
- Current workflow `.github/workflows/main.yml` should work correctly

## Verification Steps

1. **Verify Current Status**
   ```powershell
   $admin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
   Write-Host "Running as Administrator: $admin"
   ```

2. **If Admin - Run Tests**
   ```powershell
   # Build
   dotnet build PowerShellWixExtension.sln --configuration Release
   
   # Install MSIs (generates logs)
   Start-Process msiexec.exe -Wait -ArgumentList "/i Tests\PowerShellWixInlineScriptTest\bin\x86\Release\PowerShellWixInlineScriptTest.msi /q /liwearucmopvx inlinescript-install.log"
   # ... (other installs)
   
   # Run Pester
   Invoke-Pester -Path .\Tests\Pester.Tests.ps1
   ```

3. **If No Admin - Escalate**
   ```powershell
   # Windows + R: powershell
   # Right-click → "Run as Administrator"
   # Then repeat steps from option 1
   ```

## CI/CD Pipeline Status

✅ **GitHub Actions Workflow is Correct**

The current workflow in `.github/workflows/main.yml`:
- Runs on `windows-latest` runner (has admin access)
- Correctly installs MSI packages before running Pester
- Uses proper `/liwearucmopvx` verbose logging flags
- Uploads test logs as artifacts for debugging

No changes needed to workflow - it should work correctly when run in CI environment.

## Summary

| Scenario | Status | Solution |
|----------|--------|----------|
| Local dev (no admin) | ❌ Tests fail | Run PowerShell as Administrator |
| Local dev (with admin) | ✅ Tests pass | Proceed normally |
| GitHub Actions CI/CD | ✅ Should pass | Runner has admin privileges |
| Pull Request checks | ✅ Should pass | Runner has admin privileges |

The Pester tests themselves are correctly written. The test failures are expected behavior without admin privileges.
