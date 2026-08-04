# GitHub Actions Workflow Review

## File
`.github/workflows/main.yml`

## Overview
The workflow automates build, test, and release processes for PowerShellWixExtension on every push to `main` and pull request.

## Workflow Jobs

### 1. Update Release Draft (`update_release_draft`)
**Purpose**: Auto-generate release notes and version management

**Key Steps**:
1. Checkout with full history (`fetch-depth: 0`)
2. Run Nerdbank.GitVersioning (`nbgv@v0.5.2`)
   - Sets `NBGV_SemVer2` environment variable for versioning
   - Outputs: Release_Id, Release_name, Release_tag_name, Release_body, Release_html_url, Release_upload_url
3. Create draft release using release-drafter (only on main branch)

**Outputs Used By**: `build` job (dependencies)

---

### 2. Build (`build`)
**Runs After**: `update_release_draft` (depends on it)
**Runs On**: `windows-latest` (required for WiX tooling)

#### Step 1: Checkout
```yaml
uses: actions/checkout@v7
with:
  fetch-depth: 0
```

#### Step 2: Build
**Commands**:
```powershell
dotnet build .\PowerShellWixExtension\PowerShellWixExtension.csproj /p:Configuration=$env:Configuration
dotnet build PowerShellWixExtension.sln /p:Configuration=$env:Configuration
```

**Status**: ✅ **Already using dotnet (correct)**
- Uses `dotnet build` (modern SDK)
- NO `nuget restore` (not needed; dotnet build handles it)
- NO `msbuild` (not needed; dotnet build is equivalent)

#### Step 3: MSI Execution
**Commands**:
```powershell
Start-Process msiexec.exe -Wait -ArgumentList "/i Tests\PowerShellWixInlineScriptTest\bin\x86\Release\PowerShellWixInlineScriptTest.msi /q /liwearucmopvx ${{ github.workspace }}\inlinescript-install.log"
Start-Process msiexec.exe -Wait -ArgumentList "/x Tests\PowerShellWixInlineScriptTest\bin\x86\Release\PowerShellWixInlineScriptTest.msi /q /liwearucmopvx ${{ github.workspace }}\inlinescript-uninstall.log"
Start-Process msiexec.exe -Wait -ArgumentList "/i Tests\PowerShellWixTest\bin\x86\Release\PowerShellWixTest.msi /q /liwearucmopvx ${{ github.workspace }}\script-install.log"
Start-Process msiexec.exe -Wait -ArgumentList "/x Tests\PowerShellWixTest\bin\x86\Release\PowerShellWixTest.msi /q /liwearucmopvx ${{ github.workspace }}\script-uninstall.log"
```

**Status**: ✅ **Correct MSI paths** (uses `bin\x86\Release\`)
- Already updated to correct WiX 6 output paths
- Matches our AGENTS.md recommendations

**Note**: Comment explains why not using Pester directly:
> "For some reason, running msiexec from Pester doesn't work quite right."

#### Step 4: Pester Tests
```yaml
uses: zyborg/pester-tests-report@v1
with:
  include_paths: tests
  github_token: ${{ secrets.GITHUB_TOKEN }}
  tests_fail_step: true
```

**Status**: ✅ **Correct**
- Runs Pester tests against `tests` directory
- Generates test report in GitHub
- Fails workflow if tests fail (`tests_fail_step: true`)

#### Step 5: Upload Test Logs
**Action**: `actions/upload-artifact@v7`
**Path**: `${{ github.workspace }}\**\*.log`

**Status**: ✅ **Correct**
- Uploads all `.log` files as artifacts
- Runs even if previous steps fail (`if: ${{ always() }}`)
- Useful for debugging failed tests

#### Step 6: Pack (NuGet Package)
```powershell
dotnet pack .\PowerShellWixExtension.nuspec -Version "$env:NBGV_NuGetPackageVersion" -Properties "Configuration=$env:Configuration;releasenotes=$env:Release_body"
```

**Status**: ✅ **Correct dotnet usage**
- Uses `dotnet pack` (modern approach)
- NOT using `nuget pack` (legacy)
- Specifies version from NBGV: `$env:NBGV_NuGetPackageVersion`
- Includes release notes from draft release
- Configuration property passed through

#### Step 7: Upload Artifacts
**Artifact Name**: `nupkg`
**Path**: `PowerShellWixExtension.${{ env.NBGV_NuGetPackageVersion }}.nupkg`

**Status**: ✅ **Correct**
- Saves NuGet package as artifact
- File name matches the pack command output

#### Step 8: Remove Existing Release Asset (Main Only)
**Action**: `flcdrg/remove-release-asset-action@v5`
**Condition**: `if: github.ref == 'refs/heads/main'`

**Status**: ✅ **Correct**
- Only runs on main branch
- Removes old NuGet asset before uploading new one
- Allows safe re-runs

#### Step 9: Upload Release Asset (Main Only)
**Action**: `actions/upload-release-asset@v1`
**Condition**: `if: github.ref == 'refs/heads/main'`

**Status**: ✅ **Correct**
- Only runs on main branch
- Uploads NuGet package to GitHub Release
- Enables automatic GitHub release distribution

---

## Environment Variables

| Variable | Source | Used For |
|----------|--------|----------|
| `NBGV_SemVer2` | Nerdbank.GitVersioning | Release version |
| `NBGV_NuGetPackageVersion` | Nerdbank.GitVersioning | NuGet package version |
| `Configuration` | Workflow env | Build configuration (Release) |
| `Release_body` | Release draft step | Release notes for NuGet package |

---

## Permissions

```yaml
permissions:
  checks: write        # Write test results
  contents: write      # Write releases and assets
```

**Status**: ✅ **Correct minimum permissions**
- `checks: write` - Reports test results
- `contents: write` - Creates releases and uploads assets

---

## Current State vs. Recommendations

### ✅ Already Correct (No Changes Needed)

1. **Dotnet CLI Usage**
   - ✅ Uses `dotnet build` (not msbuild/nuget restore)
   - ✅ Uses `dotnet pack` (not nuget pack)

2. **Build Paths**
   - ✅ MSI paths reference `bin\x86\Release\` (WiX 6 layout)
   - ✅ No hardcoded paths from old `bin\Release\` layout

3. **Test Execution**
   - ✅ Runs MSI installation/uninstall
   - ✅ Executes Pester tests
   - ✅ Uploads test logs for debugging
   - ✅ Fails workflow on test failure

4. **Release Management**
   - ✅ Auto-generates release drafts
   - ✅ Uses semantic versioning (NBGV)
   - ✅ Includes release notes in package
   - ✅ Uploads NuGet package to GitHub Release

### ⚠️ Potential Improvements (Optional)

1. **Test Report Integration**
   - Current: Uses `zyborg/pester-tests-report@v1`
   - Consider: Update to newer Pester integration if available
   - Note: Works correctly as-is

2. **Upload Artifact Naming**
   - Current: Generic names like "test logs" and "nupkg"
   - Improvement: Could include build number/date
   - Impact: Low (artifacts auto-cleanup after retention period)

3. **Documentation for Developers**
   - Add comments explaining NBGV versioning
   - Document expected output paths
   - Note: Would help new contributors

4. **Matrix Builds**
   - Current: Single windows-latest runner
   - Future: Could add x86-specific, x64-specific, ARM64 builds
   - Status: Not applicable yet (currently x86 only)

---

## Workflow Execution Timeline

```
1. Git push to main/PR created
   ↓
2. update_release_draft job starts
   - Checkout code
   - Run nbgv versioning
   - Create draft release (main only)
   - Output: Release_Id, Release_body, Release_upload_url
   ↓
3. build job starts (depends on update_release_draft)
   - Checkout code
   - Build solution (dotnet build)
   - Run MSI install/uninstall tests
   - Run Pester tests
   - Upload test logs
   - Pack NuGet package
   - Upload NuGet artifact
   - Upload to release (main only)
```

---

## Debugging Workflow Failures

### Test Failures
1. Check uploaded test logs: `test logs` artifact
2. MSI install logs: `*-install.log`, `*-uninstall.log`
3. Pester test results: In GitHub checks/annotations

### Build Failures
1. Check dotnet build output in workflow log
2. Verify solution compiles locally: `dotnet build PowerShellWixExtension.sln --configuration Release`
3. Check for missing dependencies

### Release Upload Failures
1. Verify main branch (release only)
2. Check GitHub token permissions
3. Verify release draft was created successfully

---

## Related Files

- **Build Instructions**: `AGENTS.md` (updated to use dotnet)
- **Test Documentation**: `PESTER_TEST_REVIEW.md`
- **WiX 6 Migration**: `WIX6_MIGRATION_NOTES.md`, `WIX_FAQ_REVIEW.md`

## Summary

The GitHub Actions workflow is **well-configured and already aligned with WiX 6 best practices**:

✅ Uses modern dotnet CLI (not legacy msbuild/nuget)
✅ Correct build output paths (bin/x86/Release/)
✅ Comprehensive test coverage (MSI + Pester)
✅ Proper release automation (versioning + NuGet)
✅ Good artifact management

**No changes required** - the workflow is already optimized for the migrated codebase.
