using WixToolset.Data;

namespace PowerShellWixExtension
{
    public sealed class PowerShellScriptSymbol : IntermediateSymbol
    {
        public PowerShellScriptSymbol()
            : base(PowerShellSymbolDefinitions.PowerShellScript, null, null)
        {
        }

        public PowerShellScriptSymbol(SourceLineNumber sourceLineNumber, Identifier id = null)
            : base(PowerShellSymbolDefinitions.PowerShellScript, sourceLineNumber, id)
        {
        }
    }

    public sealed class PowerShellFileSymbol : IntermediateSymbol
    {
        public PowerShellFileSymbol()
            : base(PowerShellSymbolDefinitions.PowerShellFile, null, null)
        {
        }

        public PowerShellFileSymbol(SourceLineNumber sourceLineNumber, Identifier id = null)
            : base(PowerShellSymbolDefinitions.PowerShellFile, sourceLineNumber, id)
        {
        }
    }
}
