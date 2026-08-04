# WiX 6 Migration Notes

This document summarizes the WiX 6 migration for PowerShellWixExtension, including insights from the [FireGiant WiX FAQ](https://docs.firegiant.com/wix/whatsnew/faqs/).

## Key Changes in WiX 6

### 1. Custom Extension XML Validation

**Issue**: WiX 6 enforces strict XML schema validation at parse time, before compiler extensions run.

**Impact**: Custom extension elements cannot contain inner text or CDATA unless explicitly declared in the schema.

**Solution**: Refactored to use `Script` attribute with Base64-encoded content instead of inner text/CDATA.

```xml
<!-- WiX 4 / Legacy (no longer recommended) -->
<powershell:Script Id="Script1">
  <![CDATA[Write-Host "Hello"]]>
</powershell:Script>

<!-- WiX 6 (recommended) -->
<powershell:Script Id="Script1" Script="VwByAGkAdABlAC0ASABvAHMAdAAgACIASABlAGwAbABvACIA"/>
```

### 2. Platform-Specific Custom Actions

**Context**: WiX 6 introduces support for three platforms: x86, x64, and Arm64.

**Convention**: Custom action IDs follow the pattern: `Wix4{ActionName}_{PLATFORM_SUFFIX}`

Platform suffixes:
- `_X86` for x86
- `_X64` for x64
- `_A64` for Arm64

Example: The `QueryNativeMachine` action becomes:
- `Wix4QueryNativeMachine_X86`
- `Wix4QueryNativeMachine_X64`
- `Wix4QueryNativeMachine_A64`

**For PowerShellWixExtension**: The four custom actions follow this convention:
- `Wix4PowerShellScriptsImmediate_{PLATFORM}` (immediate)
- `Wix4PowerShellScriptsDeferred_{PLATFORM}` (deferred)
- etc.

The compiler extension handles these transparently when you reference:
- `PowerShellScriptsDeferred`
- `PowerShellScriptsElevatedDeferred`
- `PowerShellFilesDeferred`
- `PowerShellFilesElevatedDeferred`

### 3. Namespace Continuity

**Context**: Despite being version 6, WiX 6 continues using `http://wixtoolset.org/schemas/v4/wxs` namespace.

**Rationale**: Backwards compatibility and consistency with merge module ecosystem.

**For PowerShellWixExtension**: The namespace remains `http://schemas.gardiner.net.au/PowerShellWixExtensionSchema`.

### 4. Encoding Requirements

**Requirement**: Script content must be Base64-encoded UTF-16 (Unicode).

**Implementation**:
```powershell
$script = "Write-Host 'Hello'"
$bytes = [System.Text.Encoding]::Unicode.GetBytes($script)
$encoded = [Convert]::ToBase64String($bytes)
```

This encoding is enforced in `PowerShellCompilerExtension.cs` line 151:
```csharp
scriptData = Convert.ToBase64String(Encoding.Unicode.GetBytes(cdata.Value));
```

### 5. Backwards Compatibility

**Design Decision**: The compiler supports both new and legacy formats.

**New Format (WiX 6)**:
- Script attribute with Base64-encoded content
- Validated at compile time
- Works with WiX 6 strict XML validation

**Legacy Format (WiX 4 / v3)**:
- Inner text or CDATA
- Compiler automatically converts to Base64-UTF16
- May generate warnings with WiX 6

## Migration Path

### For Existing WiX 4 Projects

1. **Option A (Recommended)**: Migrate inline scripts to Base64-encoded attributes
   - Better compatibility with WiX 6
   - Eliminates XML validation warnings
   - Cleaner XML (no CDATA clutter)

2. **Option B (Transitional)**: Keep inner text format
   - Compiler still supports and converts to Base64
   - Will eventually require migration
   - Not recommended for WiX 6+

### Encoding Tool

Create a PowerShell helper for encoding:

```powershell
function Encode-PowerShellScript {
    param([string]$Script)
    [Convert]::ToBase64String([System.Text.Encoding]::Unicode.GetBytes($Script))
}

$encoded = Encode-PowerShellScript 'Write-Host "Hello"'
Write-Host $encoded  # Output: VwByAGkAdABlAC0ASABvAHMAdAAgACIASABlAGwAbABvACIA
```

## Files Changed

### PowerShellWixExtensionSchema.xsd
- **Lines 18-62**: Redesigned Script element with Base64-encoded `Script` attribute
- **Change Type**: Schema evolution (backward-compatible with compiler fallback)

### PowerShellCompilerExtension.cs
- **Lines 147-157**: Added Script attribute parsing with fallback to inner text
- **Change Type**: Compiler logic enhancement

### Test Files
- **Tests/PowerShellWixTest/Product.wxs**: 4 inline scripts converted to Script attributes
- **Tests/PowerShellWixInlineScriptTest/Product.wxs**: 2 inline scripts converted to Script attributes
- **Tests/PowerShellWixTest/ProgressDlg.wxs**: Deleted (outdated WiX 4 WixUI file)

## Build Verification

```
✅ All projects build successfully
✅ 0 errors, 0 warnings
✅ Both test MSI packages generate correctly
✅ Scripts properly serialized into custom MSI tables
```

## Related References

- [FireGiant WiX 6 FAQ](https://docs.firegiant.com/wix/whatsnew/faqs/)
- [WiX Release Notes](https://docs.firegiant.com/wix/whatsnew/releasenotes/)
- [PowerShellWixExtension Schema](PowerShellWixExtension/PowerShellWixExtensionSchema.xsd)
- [PowerShell Compiler Extension](PowerShellWixExtension/PowerShellCompilerExtension.cs)

## Known Limitations

1. **Administrator Privileges Required**: MSI installation requires admin rights (standard Windows Installer behavior)
2. **Platform Mismatch**: Must specify correct platform at build time (`-arch x86|x64|arm64`)
3. **Custom Action Naming**: Direct references to platform-specific actions require full names including prefix and suffix

## Future Considerations

1. **WiX 7 Support**: If WiX 7 introduces incompatible changes, the `Wix4` prefix may need to be updated to `Wix5`
2. **Schema Versioning**: Consider implementing XSD versioning for major breaking changes
3. **Code Generation**: Could create tooling to auto-generate Base64-encoded Script elements
