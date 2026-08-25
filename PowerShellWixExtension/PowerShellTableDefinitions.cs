using WixToolset.Data.WindowsInstaller;

namespace PowerShellWixExtension
{
    public static class PowerShellTableDefinitions
    {
        public static readonly TableDefinition PowerShellScripts = new TableDefinition(
            "PowerShellScripts",
            PowerShellSymbolDefinitions.PowerShellScript,
            new[]
            {
                new ColumnDefinition("Id", ColumnType.String, 72, true, false, ColumnCategory.Identifier),
                new ColumnDefinition("Script", ColumnType.String, 0, false, false, ColumnCategory.Text),
                new ColumnDefinition("Elevated", ColumnType.Number, 0, false, false, ColumnCategory.Integer, minValue: 0, maxValue: 1),
                new ColumnDefinition("IgnoreErrors", ColumnType.Number, 0, false, false, ColumnCategory.Integer, minValue: 0, maxValue: 1),
                new ColumnDefinition("Order", ColumnType.Number, 4, false, false, ColumnCategory.Integer, minValue: 0, maxValue: 2000000000),
                new ColumnDefinition("Condition", ColumnType.String, 0, false, true, ColumnCategory.Condition, modularizeType: ColumnModularizeType.Property),
            },
            symbolIdIsPrimaryKey: true
        );

        public static readonly TableDefinition PowerShellFiles = new TableDefinition(
            "PowerShellFiles",
            PowerShellSymbolDefinitions.PowerShellFile,
            new[]
            {
                new ColumnDefinition("Id", ColumnType.String, 72, true, false, ColumnCategory.Identifier),
                new ColumnDefinition("File", ColumnType.String, 255, false, false, ColumnCategory.Formatted, modularizeType: ColumnModularizeType.Property),
                new ColumnDefinition("Arguments", ColumnType.String, 0, false, true, ColumnCategory.Text),
                new ColumnDefinition("Elevated", ColumnType.Number, 0, false, false, ColumnCategory.Integer, minValue: 0, maxValue: 1),
                new ColumnDefinition("IgnoreErrors", ColumnType.Number, 0, false, false, ColumnCategory.Integer, minValue: 0, maxValue: 1),
                new ColumnDefinition("Order", ColumnType.Number, 4, false, false, ColumnCategory.Integer, minValue: 0, maxValue: 2000000000),
                new ColumnDefinition("Condition", ColumnType.String, 0, false, true, ColumnCategory.Condition, modularizeType: ColumnModularizeType.Property),
            },
            symbolIdIsPrimaryKey: true
        );

        public static readonly TableDefinition[] All =
        {
            PowerShellScripts,
            PowerShellFiles,
        };
    }
}
