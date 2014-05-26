using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

using Microsoft.Deployment.WindowsInstaller;

namespace PowerShellActions
{
    internal class WixHostUserInterface : PSHostUserInterface
    {
        private readonly Session _session;
        private readonly WixHostRawUserInterface _wixHostRawUserInterface;
        private string _progressActivity;

        public WixHostUserInterface(Session session)
        {
            _session = session;
            _wixHostRawUserInterface = new WixHostRawUserInterface();
            _progressActivity = string.Empty;
        }

        [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules", Justification = "Reviewed. Suppression is OK here.")]
        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return _wixHostRawUserInterface;
            }
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new NotImplementedException("PromptForChoice");
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException("PromptForCredential");
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException("PromptForCredential");
        }

        public override string ReadLine()
        {
            throw new NotImplementedException("ReadLine");
        }

        public override SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException("ReadLineAsSecureString");
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            _session.Log(value);
        }

        public override void Write(string value)
        {
            _session.Log(value);
        }

        public override void WriteDebugLine(string message)
        {
            _session.Log(message);
        }

        public override void WriteErrorLine(string value)
        {
            var record = new Record(0);
            record[0] = value;
            _session.Message(InstallMessage.Error, record);
        }

        public override void WriteLine(string value)
        {
            _session.Log(value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
            Record hActionRec = new Record(3);
            Record hProgressRec = new Record(3);

            // Installer is executing the installation script. Set up a
            // record specifying appropriate templates and text for
            // messages that will inform the user about what the custom
            // action is doing. Tell the installer to use this template and
            // text in progress messages.
            hActionRec[1] = progressRecord.PercentComplete;
            hActionRec[2] = 100; // 100%
            hActionRec[3] = progressRecord.CurrentOperation;

            // Specify that an update of the progress bar’s position in
            // this case means to move it forward by one increment.
            hProgressRec[1] = 2;
            hProgressRec[2] = CustomActions.TickIncrement;
            hProgressRec[3] = 0;

            _session.Message(InstallMessage.ActionData, hActionRec);
            _session.Message(InstallMessage.Progress, hProgressRec);
        }

        public override void WriteVerboseLine(string message)
        {
            _session.Log(message);
        }

        public override void WriteWarningLine(string message)
        {
            var record = new Record(0);
            record[0] = message;
            _session.Message(InstallMessage.Warning, record);
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException("Prompt");
        }
    }
}