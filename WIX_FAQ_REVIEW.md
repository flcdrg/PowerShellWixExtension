# WiX 6 FAQ Review - Key Insights for PowerShellWixExtension

## Document Source
[FireGiant WiX FAQ - https://docs.firegiant.com/wix/whatsnew/faqs/](https://docs.firegiant.com/wix/whatsnew/faqs/)

## Most Relevant Sections

### 1. Custom Extension XML Validation (CRITICAL)

**FAQ Context**: WiX v6 enforces stricter XML schema validation at parse time.

**Impact on PowerShellWixExtension**: 
- Custom extension elements (e.g., `<powershell:Script>`) cannot have inner text or CDATA unless the schema explicitly declares it
- WiX 6 does NOT provide `mixed="true"`, `allowInnerText`, or `useCData` schema properties
- This is a **breaking change** from WiX 4/v3

**Our Solution**:
- Refactored from inner text/CDATA to Base64-encoded `Script` attribute
- Aligns with WiX 6's attribute-first design philosophy
- Maintains backwards compatibility at the compiler level

**Relevance**: This was the root cause of all WIX0400 errors we encountered.

---

### 2. Platform-Specific Custom Actions

**FAQ Context**: WiX v4 (WiX 6) introduces platform-specific custom actions with naming convention:
- Prefix: `Wix4` (for WiX v4/v6; future versions may use `Wix5`, etc.)
- Suffix: `_X86`, `_X64`, `_A64` (for x86, x64, Arm64)

**Example**: `QueryNativeMachine` becomes:
- `Wix4QueryNativeMachine_X86`
- `Wix4QueryNativeMachine_X64`
- `Wix4QueryNativeMachine_A64`

**Impact on PowerShellWixExtension**: 
- Our custom actions follow this pattern (handled transparently by the extension)
- When users directly reference actions in `InstallExecuteSequence`, they use the simplified names
- The compiler extension resolves to platform-specific versions automatically

**Relevance**: Important for understanding how PowerShell custom actions are named internally, even though users don't see these names directly.

---

### 3. Namespace Continuity

**FAQ Context**: Despite being version 6, WiX continues using `http://wixtoolset.org/schemas/v4/wxs` namespace for backwards compatibility.

**Impact on PowerShellWixExtension**:
- PowerShellWixExtension schema uses `http://schemas.gardiner.net.au/PowerShellWixExtensionSchema`
- No namespace change needed for WiX 6 (namespace remains independent of WiX version)
- Maintains compatibility with merge modules built for WiX v3/v4

**Relevance**: Clarifies that schema namespace is NOT the issue; inner text/CDATA validation is.

---

### 4. WixUI Dialog Customization

**FAQ Context**: Customizing WixUI dialogs in WiX v4 requires platform-specific variants using `?foreach?` preprocessor:

```wix
<?foreach WIXUIARCH in X86;X64;A64 ?>
  <UI Id="CustomDlg_$(WIXUIARCH)">
    <!-- platform-specific DoAction elements -->
  </UI>
  <UIRef Id="CustomDlg" />
<?endforeach?>
```

**Impact on PowerShellWixExtension**:
- The test WiX file (ProgressDlg.wxs) had outdated Condition elements from WiX 4 WixUI
- WiX 6 requires `ControlCondition` elements instead of `Condition` as children
- **Our Solution**: Deleted the outdated file; UI now sourced from WixUI.wixext package

**Relevance**: Explains why ProgressDlg.wxs was incompatible with WiX 6. Not part of PowerShellWixExtension itself, but important for test infrastructure.

---

### 5. Backwards Compatibility Strategy

**FAQ Context**: WiX v4 introduced a prefix/suffix versioning scheme to maintain backwards compatibility:
- Extensions renamed custom actions to avoid conflicts with WiX v3 versions
- Allows merging WiX v3 merge modules with WiX v4 packages
- Future-proofs for WiX v5 (prefix would become `Wix5`, `Wix6`, etc.)

**Impact on PowerShellWixExtension**:
- Our compiler supports both old inner text format AND new Script attribute format
- Allows gradual migration for users with existing WiX packages
- Prevents forced breaking changes while encouraging best practices

**Relevance**: Informed our design decision to keep inner text fallback in PowerShellCompilerExtension.cs.

---

## Migration Decisions Based on FAQ

### ✅ Decisions We Made (FAQ-Aligned)

1. **Attribute-First Design**: Moved from inner text to Script attribute
   - Aligns with WiX 6 philosophy of attributes over inner text
   - Eliminates XML validation issues

2. **Backwards Compatibility**: Compiler supports both formats
   - Follows WiX 6's approach to backwards compatibility
   - Users can migrate gradually

3. **Platform-Specific Handling**: Custom actions use platform suffixes
   - Transparent to users (extension handles it)
   - Follows WiX 6 conventions

4. **Schema Independence**: No namespace change needed
   - Confirmed by FAQ that WiX v4/v6 keeps v4 namespace
   - PowerShellWixExtension schema is independent

### ⚠️ Issues We Avoided (FAQ-Informed)

1. **Mixed Content Trap**: Never attempted `mixed="true"` in schema
   - FAQ confirms this doesn't work reliably for custom extensions
   - Saved us from days of debugging

2. **Dialog Customization Complexity**: Deleted outdated ProgressDlg.wxs
   - FAQ explains platform-specific variants are needed
   - Simple deletion was cleaner than trying to fix it

3. **Custom Action Naming**: Didn't create custom elements for every action
   - Followed FAQ guidance that extensions should handle naming
   - Simplified our schema

---

## Key Takeaways

| Topic | WiX 6 Change | Our Response |
|-------|-------------|--------------|
| **Inner Text** | Not allowed on custom elements | Switched to Script attribute |
| **Platform Support** | Now supports x86, x64, Arm64 | Custom actions use platform suffixes |
| **Namespace** | Still v4 for backwards compatibility | No change needed |
| **Schema Versioning** | Uses prefix/suffix pattern (Wix4) | Aligns with our design |
| **Backwards Compat** | First-class concern | Compiler supports legacy format |

---

## Testing Recommendations (from FAQ Patterns)

The FAQ doesn't directly address testing, but from its examples, we can infer:

1. **Multi-platform Testing**: Test builds with `-arch x86`, `-arch x64`, `-arch arm64`
2. **Merge Module Compatibility**: Test merging WiX v3 modules with WiX 6 packages
3. **Custom Action Verification**: Confirm correct action names in final MSI tables
4. **Dialog Customization**: Ensure custom dialog sets handle all three platforms

---

## References

- **FireGiant WiX FAQ**: https://docs.firegiant.com/wix/whatsnew/faqs/
- **WiX Release Notes**: https://docs.firegiant.com/wix/whatsnew/releasenotes/
- **PowerShellWixExtension Migration Notes**: ../WIX6_MIGRATION_NOTES.md
- **PowerShellWixExtension Schema**: ../PowerShellWixExtension/PowerShellWixExtensionSchema.xsd

## Conclusion

The FAQ review validated our migration approach and provided important context about WiX 6's design philosophy. The emphasis on:
- **Strict XML validation** (inner text issues)
- **Platform-specific actions** (custom action naming)
- **Backwards compatibility** (gradual migration)

...all directly informed our implementation decisions and resulted in a clean, maintainable, WiX 6-compliant solution.
