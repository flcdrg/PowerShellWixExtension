using System;
using System.Reflection;
using Microsoft.Tools.WindowsInstallerXml;

namespace PowerShellWixExtension
{
    public class PowerShellWixExtension : WixExtension
    {
        private CompilerExtension _compilerExtension;
        private Library _library;
        private TableDefinitionCollection _tableDefinitions;

        public override CompilerExtension CompilerExtension
        {
            get
            {
                return _compilerExtension ?? ( _compilerExtension = new PowerShellCompilerExtension() );
            }
        }

        public override TableDefinitionCollection TableDefinitions
        {
            get
            {
                return _tableDefinitions ?? ( _tableDefinitions = LoadTableDefinitionHelper( Assembly.GetExecutingAssembly(), "PowerShellWixExtension.TableDefinitions.xml" ) );
            }
        }

        public override Library GetLibrary( TableDefinitionCollection tableDefinitions )
        {
            return _library ?? ( _library = LoadLibraryHelper( Assembly.GetExecutingAssembly(), "PowerShellWixExtension.PowerShellLibrary.wixlib", tableDefinitions ) );
        }
    }
}