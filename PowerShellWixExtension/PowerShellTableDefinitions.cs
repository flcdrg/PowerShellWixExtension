using PowerShellWixExtension.Symbols;

using WixToolset.Data.WindowsInstaller;

namespace PowerShellWixExtension
{
    public static class PowerShellTableDefinitions
    {
        public static readonly TableDefinition PowerShellScripts = new TableDefinition(
            "PowerShellScripts",
            PowerShellSymbolDefinitions.PowerShellScripts,
            new[]
            {
                    new ColumnDefinition("Id", ColumnType.String, 72, primaryKey: true, nullable: false, ColumnCategory.Identifier, description: "Primary key for this element", modularizeType: ColumnModularizeType.Column),
                    new ColumnDefinition("Script", ColumnType.String, 0, primaryKey: false, nullable: false, ColumnCategory.Text, description: "The PowerShell script to execute", modularizeType: ColumnModularizeType.Property),
                    new ColumnDefinition("Elevated", ColumnType.Number, 0, primaryKey: false, nullable: false, ColumnCategory.Unknown, minValue: 0, maxValue: 1, description: "Run as elevated", modularizeType: ColumnModularizeType.Property),
                    new ColumnDefinition("IgnoreErrors", ColumnType.Number, 0, primaryKey: false, nullable: false, ColumnCategory.Integer, minValue: 0, maxValue: 1, description: "When non-zero, PowerShell errors are ignored."),
                    new ColumnDefinition("Order", ColumnType.Number, 4, primaryKey: false, nullable: false, ColumnCategory.Integer, minValue: 0, maxValue: 2000000000, description: "Order of execution."),
                    new ColumnDefinition("Condition", ColumnType.String, 0, primaryKey: false, nullable: true, ColumnCategory.Condition, modularizeType: ColumnModularizeType.Property)
            },
            symbolIdIsPrimaryKey: true
        );

        public static readonly TableDefinition PowerShellFiles = new TableDefinition(
            "PowerShellFiles",
            PowerShellSymbolDefinitions.PowerShellFiles,
            new[]
            {
                    new ColumnDefinition("Id", ColumnType.String, 72, primaryKey: true, nullable: false, ColumnCategory.Identifier, description: "Primary key for this element", modularizeType: ColumnModularizeType.Column),
                    new ColumnDefinition("FilePath", ColumnType.String, 0, primaryKey: false, nullable: false, ColumnCategory.Text, description: "The path to the PowerShell file", modularizeType: ColumnModularizeType.Property),
                    new ColumnDefinition("Arguments", ColumnType.String, 0, primaryKey: false, nullable: true, ColumnCategory.Text, description: "Arguments to pass to the PowerShell script", modularizeType: ColumnModularizeType.Property),
                    new ColumnDefinition("Elevated", ColumnType.Number, 0, primaryKey: false, nullable: false, ColumnCategory.Unknown, minValue: 0, maxValue: 1, description: "Run as elevated", modularizeType: ColumnModularizeType.Property),
                    new ColumnDefinition("IgnoreErrors", ColumnType.Number, 0, primaryKey: false, nullable: false, ColumnCategory.Integer, minValue: 0, maxValue: 1, description: "When non-zero, PowerShell errors are ignored."),
                    new ColumnDefinition("Order", ColumnType.Number, 4, primaryKey: false, nullable: false, ColumnCategory.Integer, minValue: 0, maxValue: 2000000000, description: "Order of execution."),
                    new ColumnDefinition("Condition", ColumnType.String, 0, primaryKey: false, nullable: true, ColumnCategory.Condition, modularizeType: ColumnModularizeType.Property)
            },
            symbolIdIsPrimaryKey: true
        );

        public static readonly TableDefinition[] All = new[]
        {
                PowerShellScripts,
                PowerShellFiles
            };
    }
}