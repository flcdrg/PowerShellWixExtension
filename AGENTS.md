# Copilot Instructions for PowerShellWixExtension

## Build, test, and lint commands

This repository is built and tested on **Windows** (WiX + MSI tooling required).

```powershell
# Restore
nuget restore

# Build all projects (same command used in CI)
msbuild PowerShellWixExtension.sln /p:Configuration=Release
```

Integration tests are MSI install/uninstall + Pester log assertions:

```powershell
# Build test MSIs first, then run install/uninstall like CI
Start-Process msiexec.exe -Wait -ArgumentList "/i Tests\PowerShellWixInlineScriptTest\bin\Release\PowerShellWixInlineScriptTest.msi /q /liwearucmopvx $pwd\inlinescript-install.log"
Start-Process msiexec.exe -Wait -ArgumentList "/x Tests\PowerShellWixInlineScriptTest\bin\Release\PowerShellWixInlineScriptTest.msi /q /liwearucmopvx $pwd\inlinescript-uninstall.log"
Start-Process msiexec.exe -Wait -ArgumentList "/i Tests\PowerShellWixTest\bin\Release\PowerShellWixTest.msi /q /liwearucmopvx $pwd\script-install.log"
Start-Process msiexec.exe -Wait -ArgumentList "/x Tests\PowerShellWixTest\bin\Release\PowerShellWixTest.msi /q /liwearucmopvx $pwd\script-uninstall.log"

# Run Pester assertions
Invoke-Pester -Path .\Tests\Pester.Tests.ps1
```

Run a single Pester test:

```powershell
# Pester 5 style
Invoke-Pester -Path .\Tests\Pester.Tests.ps1 -FullNameFilter "Inline Scripts.Install"

# Pester 4 style
Invoke-Pester -Path .\Tests\Pester.Tests.ps1 -TestName "Install"
```

Linting: there is **no separate lint command wired in CI**. `Settings.StyleCop` exists for Visual Studio/StyleCop configuration, but CI runs restore/build/test/pack.

## High-level architecture

- `PowerShellWixExtension` is the WiX extension assembly loaded by candle/light:
  - `PowerShellCompilerExtension` parses `<powershell:Script>` and `<powershell:File>` under `Product`/`Fragment`.
  - Parsed data is written into custom MSI tables (`PowerShellScripts`, `PowerShellFiles`) defined by `TableDefinitions.xml`.
  - The extension embeds and returns `PowerShellLibrary.wixlib`, which contains all custom action declarations and default sequencing.
- `PowerShellLibrary/Library.wxs` defines immediate + deferred install/uninstall custom actions for:
  - inline scripts vs script files
  - elevated vs non-elevated execution
  - default sequencing and UI progress text
- `PowerShellActions` is the custom action runtime:
  - Immediate actions query MSI tables, evaluate per-row `Condition`, serialize payload into deferred `CustomActionData`, and extend progress ticks.
  - Deferred actions deserialize payload and execute PowerShell through `PowerShellTask` + `WixHost*` classes, logging via MSI session and honoring `IgnoreErrors`.
- `Tests/*/*.wixproj` build sample MSIs that consume the extension from `Libs\PowerShellWixExtension.dll`; `Tests/Pester.Tests.ps1` validates behavior by inspecting MSI log output.

## Key repository conventions

- Keep the **immediate/deferred pair pattern** intact for any new custom action behavior: immediate actions prepare validated data, deferred actions perform execution.
- Inline script payloads are stored as **Base64-encoded UTF-16** (`Encoding.Unicode`) in the `PowerShellScripts.Script` column; preserve this encoding path end-to-end.
- Execution ordering uses `Order` with implicit default `1000000000 + source line`; explicit lower `Order` values run first within each context/type bucket.
- `Elevated`, `IgnoreErrors`, and `Condition` are first-class schema/table/runtime fields; changes to extension schema must stay synchronized across:
  1. `PowerShellWixExtensionSchema.xsd`
  2. `TableDefinitions.xml`
  3. compiler parsing (`PowerShellCompilerExtension`)
  4. runtime query/execution logic (`PowerShellActions/CustomAction.cs`)
- Test WiX projects resolve the extension from `..\..\Libs\PowerShellWixExtension.dll`; keep `AfterBuild` copy steps in project files working when changing output layout.
- For inline scripts in `.wxs`, square brackets must be escaped as `[\[]` and `[\]]` to avoid MSI property expansion.
