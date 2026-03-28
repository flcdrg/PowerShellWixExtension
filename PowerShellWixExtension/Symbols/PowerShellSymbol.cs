using WixToolset.Data;

namespace PowerShellWixExtension.Symbols
{
    public static partial class PowerShellSymbolDefinitions
    {
        public static readonly IntermediateSymbolDefinition PowerShellScripts = new IntermediateSymbolDefinition(
                       PowerShellSymbolDefinitionType.PowerShellScripts.ToString(),
                                  new[]
                                  {
                new IntermediateFieldDefinition(nameof(PowerShellScriptsSymbolFields.Id), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(PowerShellScriptsSymbolFields.Script), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(PowerShellScriptsSymbolFields.Elevated), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellScriptsSymbolFields.IgnoreErrors), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellScriptsSymbolFields.Order), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellScriptsSymbolFields.Condition), IntermediateFieldType.String),
            },
            typeof(PowerShellScriptSymbol)
        );

        public static IntermediateSymbolDefinition PowerShellFiles = new IntermediateSymbolDefinition(
                       PowerShellSymbolDefinitionType.PowerShellFiles.ToString(),
                                  new[]
                                  {
                new IntermediateFieldDefinition(nameof(PowerShellFilesSymbolFields.Id), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(PowerShellFilesSymbolFields.FilePath), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(PowerShellFilesSymbolFields.Arguments), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(PowerShellFilesSymbolFields.Elevated), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellFilesSymbolFields.IgnoreErrors), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellFilesSymbolFields.Order), IntermediateFieldType.Number),
                new IntermediateFieldDefinition(nameof(PowerShellFilesSymbolFields.Condition), IntermediateFieldType.String),
            },
            typeof(PowerShellFileSymbol)
        );
    }

    public enum PowerShellScriptsSymbolFields
    {
        Id,
        Script,
        Elevated,
        IgnoreErrors,
        Order,
        Condition
    }

    public class PowerShellScriptSymbol : IntermediateSymbol
    {
        public PowerShellScriptSymbol(IntermediateSymbolDefinition definition) : base(definition)
        {
        }

        public PowerShellScriptSymbol(IntermediateSymbolDefinition definition, SourceLineNumber sourceLineNumber, Identifier id = null) : base(definition, sourceLineNumber, id)
        {
        }

        public IntermediateField this[PowerShellScriptsSymbolFields index] => Fields[(int)index];

        public string Id
        {
            get => Fields[(int)PowerShellScriptsSymbolFields.Id].ToString();
            set => this.Set((int)PowerShellScriptsSymbolFields.Id, value);
        }

        public string Script
        {
            get => Fields[(int)PowerShellScriptsSymbolFields.Script].ToString();
            set => this.Set((int)PowerShellScriptsSymbolFields.Script, value);
        }

        public int Elevated
        {
            get => (int)Fields[(int)PowerShellScriptsSymbolFields.Elevated];
            set => this.Set((int)PowerShellScriptsSymbolFields.Elevated, value);
        }

        public int IgnoreErrors
        {
            get => (int)Fields[(int)PowerShellScriptsSymbolFields.IgnoreErrors];
            set => this.Set((int)PowerShellScriptsSymbolFields.IgnoreErrors, value);
        }

        public int Order
        {
            get => (int)Fields[(int)PowerShellScriptsSymbolFields.Order];
            set => this.Set((int)PowerShellScriptsSymbolFields.Order, value);
        }

        public string Condition
        {
            get => Fields[(int)PowerShellScriptsSymbolFields.Condition].ToString();
            set => this.Set((int)PowerShellScriptsSymbolFields.Condition, value);
        }
    }

    public enum PowerShellFilesSymbolFields
    {
        Id,
        FilePath,
        Arguments,
        Elevated,
        IgnoreErrors,
        Order,
        Condition,
    }

    public class PowerShellFileSymbol : IntermediateSymbol
    {
        public PowerShellFileSymbol(IntermediateSymbolDefinition definition) : base(definition)
        {
        }

        public PowerShellFileSymbol(IntermediateSymbolDefinition definition, SourceLineNumber sourceLineNumber,
            Identifier id = null) : base(definition, sourceLineNumber, id)
        {
        }

        public IntermediateField this[PowerShellFilesSymbolFields index] => Fields[(int) index];

        public string Id
        {
            get => Fields[(int) PowerShellFilesSymbolFields.Id].ToString();
            set => this.Set((int) PowerShellFilesSymbolFields.Id, value);
        }

        public string FilePath
        {
            get => Fields[(int) PowerShellFilesSymbolFields.FilePath].ToString();
            set => this.Set((int) PowerShellFilesSymbolFields.FilePath, value);
        }

        public string Arguments
        {
            get => Fields[(int) PowerShellFilesSymbolFields.Arguments].ToString();
            set => this.Set((int) PowerShellFilesSymbolFields.Arguments, value);
        }

        public int Elevated
        {
            get => (int)Fields[(int) PowerShellFilesSymbolFields.Elevated];
            set => this.Set((int) PowerShellFilesSymbolFields.Elevated, value);
        }

        public int IgnoreErrors
        {
            get => (int)Fields[(int) PowerShellFilesSymbolFields.IgnoreErrors];
            set => this.Set((int) PowerShellFilesSymbolFields.IgnoreErrors, value);
        }

        public int Order
        {
            get => (int)Fields[(int) PowerShellFilesSymbolFields.Order];
            set => this.Set((int) PowerShellFilesSymbolFields.Order, value);
        }

        public string Condition
        {
            get => Fields[(int) PowerShellFilesSymbolFields.Condition].ToString();
            set => this.Set((int) PowerShellFilesSymbolFields.Condition, value);
        }
    }
}